using System.ComponentModel.DataAnnotations;

namespace ConverterApplication.Models;

public class Asset
{
    public Guid Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    public int ContractId { get; set; }
    
    [Required]
    public required string Category { get; set; }
} 