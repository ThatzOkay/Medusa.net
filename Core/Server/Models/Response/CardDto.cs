using Abstractions.Entities;
using Facet;
using System;

namespace Server.Models.Response;

[Facet(typeof(Card), exclude: nameof(Card.User))]
public partial class CardDto
{
}
