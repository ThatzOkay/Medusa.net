using Microsoft.EntityFrameworkCore;
using Server.Utils;
using System.Text;
using Abstractions.Entities;
using Abstractions.Services;

namespace Server.Services;

public class CardService : ICardService
{
    private const string Alphabet = "0123456789ABCDEFGHJKLMNPRSTUWXYZ";
    private readonly byte[] _rawKey = "?I'llB2c.YouXXXeMeHaYpy!"u8.ToArray();
    private readonly DesEncryption _desEncryption;

    private readonly ILogger<CardService> _logger;
    private readonly AppDbContext _context;

    public CardService(ILogger<CardService> logger, AppDbContext context)
    {
        var encryptionKey = _rawKey.Select(x => (byte)(x * 2)).ToArray();
        _desEncryption = new DesEncryption(encryptionKey);
        _logger = logger;
        _context = context;
    }

    public string ConvertKonamiIdToUid(string konamiId)
    {
        if(konamiId.Length != 16)
        {
            _logger.LogError("Invalid Konami ID length must be 16 characters");
            return string.Empty;
        }

        var cardType = konamiId[14] == '1' ? 1 : konamiId[14] == '2' ? 2 : -1;

        if(cardType == -1)
        {
            _logger.LogError("Invalid Konami ID");
            return string.Empty;
        }

        var card = konamiId.Select(k => (byte)Alphabet.IndexOf(k)).ToArray();

        if(card[11] % 2 != card[12] % 2 || card[13] != (card[12] ^ 1) || card[15] != CalculateChecksum(card))
        {

            _logger.LogError("Invalid card data");
            return string.Empty;
        }

        for(var i = 13; i > 0; i--)
        {
            card[i] ^= card[i - 1];
        }

        card[0] ^= (byte)cardType;

        var packed = Pack5(card.Take(13).ToArray()).Take(8).ToArray();
        var decrypted = _desEncryption.Decrypt(packed).Reverse().ToArray();

        var cardId = Convert.ToHexString(decrypted).ToUpper();

        if ((cardType != 1 || cardId.StartsWith("E004")) && (cardType != 2 || cardId[0] == '0')) return cardId;
        _logger.LogError("Invalid card `type");
        return string.Empty;

    }

    public string ConvertUidToKonamiId(string uid)
    {
        if(uid.Length != 16)
        {
            _logger.LogError("Invalid UID length must be 16 characters");
            return string.Empty;
        }

        var cardType = uid.StartsWith("E004") ? 1 : uid[0] == '0' ? 2 : -1;

        if(cardType == -1)
        {
            _logger.LogError("Invalid UID prefix");
            return string.Empty;
        }

        var konamiId = Enumerable.Range(0, uid.Length / 2)
            .Select(x => Convert.ToByte(uid.Substring(x * 2, 2), 16))
            .ToArray();

        konamiId = konamiId.Reverse().ToArray();

        if(konamiId.Length != 8)
        {
            _logger.LogError("Invalid UID length. Must be 8 bytes");
            return string.Empty;
        }

        var encryptedId = _desEncryption.Encrypt(konamiId);

        if(encryptedId.Length != 8)
        {
            _logger.LogError("Invalid ID length. Must be 8 bytes");
            return string.Empty;
        }

        var unpackedId = Unpack5(encryptedId);
        unpackedId = unpackedId.Take(13).Concat("\0\0\0"u8.ToArray()).ToArray();

        if(unpackedId.Length != 16)
        {
            _logger.LogError("Invalid unpacked ID length. Must be 16 bytes");
            return string.Empty;
        }

        unpackedId[0] ^= (byte)cardType;
        unpackedId[13] = 1;

        for(var i = 1; i < 14; i++)
        {
            unpackedId[i] ^= unpackedId[i - 1];
        }

        unpackedId[14] = (byte)cardType;
        unpackedId[15] = CalculateChecksum(unpackedId);

        return string.Concat(unpackedId.Select(u => Alphabet[u]));
    }

    private static byte[] Unpack5(byte[] data)
    {
        // Convert each byte to an 8-bit binary string and concatenate them
        var binaryString = new StringBuilder();
        foreach(var b in data)
        {
            binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
        }

        // Ensure the total length is a multiple of 5 by padding with zeroes if necessary
        var remainder = binaryString.Length % 5;
        if(remainder != 0)
        {
            binaryString.Append(new string('0', 5 - remainder));
        }

        // Convert the 5-bit chunks back into bytes
        var result = new List<byte>();
        for(var i = 0; i < binaryString.Length; i += 5)
        {
            var chunk = binaryString.ToString(i, 5);
            result.Add(Convert.ToByte(chunk, 2));
        }

        return [.. result];
    }

    private static byte[] Pack5(byte[] data)
    {
        var packed = new StringBuilder();
        foreach(var b in data)
        {
            packed.Append(Convert.ToString(b, 2).PadLeft(5, '0'));
        }

        // Padding if not a multiple of 8
        if(packed.Length % 8 != 0)
        {
            packed.Append('0', 8 - packed.Length % 8);
        }

        return Enumerable.Range(0, packed.Length / 8)
            .Select(i => Convert.ToByte(packed.ToString().Substring(i * 8, 8), 2))
            .ToArray();
    }

    private static byte CalculateChecksum(byte[] data)
    {
        var checksum = 0;
        for(var i = 0; i < 15; i++)
        {
            if(i >= data.Length)
            {
                break;
            }
            checksum += data[i] * ((i % 3) + 1);
        }

        // Reduce the checksum to fit within 5 bits
        while(checksum > 31)
        {
            checksum = (checksum >> 5) + (checksum & 31);
        }

        // Return the checksum as a byte
        return (byte)checksum;

    }

    public Task<Card?> FindByKonamiId(string konamiId)
    {
        return _context.Cards.Include(x => x.User).FirstOrDefaultAsync(c => c.KonamiId == konamiId);
    }

    public Task<Card?> FindByCardId(string cardId)
    {
        return _context.Cards.Include(x => x.User).FirstOrDefaultAsync(c => c.RawId == cardId);
    }

    public Task<bool> Exists(string konamiId)
    {
        return _context.Cards.AnyAsync(c => c.KonamiId == konamiId);
    }

    public Task<bool> ValidatePinAsync(string konamiId, string pin)
    {
        return _context.Cards.AnyAsync(c => c.KonamiId == konamiId && c.User.Pin == pin);
    }

    public Task<bool> IsRegistered(string konamiId, string pin)
    {
        return _context.Cards.AnyAsync(c => c.KonamiId == konamiId && c.User.Pin == pin && c.User.PasswordHash != null);
    }

    public Task<List<Card>> GetCardsByUserIdAsync(int userId)
    {
        return _context.Cards.Where(c => c.UserId == userId).ToListAsync();
    }
}
