using System.Text.Json.Serialization;

namespace ConverterApplication.Domain.Models;


public class Contract
{
    public int ContractId { get; set; }
    public int CompanyId { get; set; }
    public DateTime SignatureDate { get; set; }
    public UserData UserData { get; set; } = new();
}
