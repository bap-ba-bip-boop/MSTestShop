﻿using System.ComponentModel.DataAnnotations;

namespace MvcSuperShop.Data;

public class Category
{
    public int Id { get; set; } 
        
    [MaxLength(50)]public string? Name { get; set; }
    [MaxLength(50)] public string? Icon { get; set; }
}