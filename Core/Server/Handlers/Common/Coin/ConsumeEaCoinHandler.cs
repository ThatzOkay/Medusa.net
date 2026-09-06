using Abstractions.Handlers;
using Microsoft.EntityFrameworkCore;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Coin;

public class ConsumeEaCoinHandler(IEaCoinSessionService sessionService, AppDbContext dbContext) : Handler<EaCoinConsumeRequest, EaCoinConsumeResponse>
{
    public override void Configure()
    {
        Module("eacoin");
        Method("consume");
    }

    public override async Task<EaCoinConsumeResponse> HandleAsync(EaCoinConsumeRequest req, GameModel model)
    {
        var session = sessionService.GetSession(req.sessionId);

        if (session == null)
        {
            return new EaCoinConsumeResponse()
            {
                Status = -4, // EA3_EACOIN_ERROR.NOTCHECKIN
                AcStatus = 0,
                AutoCharge = 0,
                Balance = 0
            };
        }

        if (session.IsCharging)
        {
            return new EaCoinConsumeResponse()
            {
                Status = -2, // EA3_EACOIN_ERROR.SLOTINUSE
                AcStatus = 0,
                AutoCharge = 0,
                Balance = 0
            };
        }

        session.IsCharging = true;

        var user = await dbContext.Users.FindAsync(session.UserId);

        if (user == null)
        {
            session.IsCharging = false;
            return new EaCoinConsumeResponse()
            {
                Status = -7, // EA3_EACOIN_ERROR.NOACCOUNT
                AcStatus = 0,
                AutoCharge = 0,
                Balance = 0
            };
        }

        if (user.PaseliAmount < req.Payment)
        {
            session.IsCharging = false;
            return new EaCoinConsumeResponse()
            {
                Status = -8, // EA3_EACOIN_ERROR.SHORTBALANCE
                AcStatus = 0,
                AutoCharge = 0,
                Balance = user.PaseliAmount
            };
        }

        user.PaseliAmount -= req.Payment;
        await dbContext.SaveChangesAsync();

        session.IsCharging = false;
        return new EaCoinConsumeResponse()
        {
            Status = 0,
            AcStatus = 0,
            AutoCharge = 0,
            Balance = user.PaseliAmount
        };
    }
}