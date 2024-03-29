﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MvcSuperShop.Data;


[Index(nameof(UserAgreements.Email))]
public class UserAgreements
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string? Email { get; set; }

    public Agreement? Agreement { get; set; }
}