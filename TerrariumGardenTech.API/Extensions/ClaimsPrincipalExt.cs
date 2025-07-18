﻿using System.Security.Claims;

namespace TerrariumGardenTech.API.Extensions;

public static class ClaimsPrincipalExt
{
    public static int GetUserId(this ClaimsPrincipal u)
    {
        return int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}