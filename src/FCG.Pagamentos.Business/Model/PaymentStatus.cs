namespace FCG.Pagamentos.Business.Model;

public enum PaymentStatus
{
    PENDING,
    ANALISE,
    REJECTED,
    SUCESSO,
    CANCELED,
    ERROR
}

public static class PaymentStatusMapper
{
    public static PaymentStatus FromString(string? status)
    {
        var s = (status ?? string.Empty).Trim().ToUpperInvariant();
        return s switch
        {
            "PENDING" or "PENDENTE" => PaymentStatus.PENDING,
            "ANALISE" or "ANALYSIS" => PaymentStatus.ANALISE,
            "REJECTED" or "REJEITADO" => PaymentStatus.REJECTED,
            "SUCESSO" or "APPROVED" or "APROVADO" => PaymentStatus.SUCESSO,
            "CANCELED" or "CANCELLED" or "CANCELAR" => PaymentStatus.CANCELED,
            _ => PaymentStatus.ERROR
        };
    }

    public static string ToStringValue(PaymentStatus status) => status switch
    {
        PaymentStatus.PENDING => "PENDING",
        PaymentStatus.ANALISE => "ANALISE",
        PaymentStatus.REJECTED => "REJECTED",
        PaymentStatus.SUCESSO => "SUCESSO",
        PaymentStatus.CANCELED => "CANCELED",
        _ => "ERROR"
    };
}

public enum EventType
{
    Payment_Consulted,
    Payment_Approved,
    Payment_Cancelled,
    Payment_Pending,
    Payment_Rejected,
    Payment_In_Analisys,
    Payment_Updated,
    Payment_Error,
    Payment_Not_Found,
    Payment_Purchase_By_User,
    Payment_Purchase_Not_Found
}

public static class PaymentEventTypeMapper
{
    public static string FromStatusString(string? status)
    {
        var normalized = PaymentStatusMapper.FromString(status);
        return FromStatus(normalized);
    }

    public static string FromStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.PENDING => nameof(EventType.Payment_Pending),
        PaymentStatus.ANALISE => nameof(EventType.Payment_In_Analisys),
        PaymentStatus.REJECTED => nameof(EventType.Payment_Rejected),
        PaymentStatus.SUCESSO => nameof(EventType.Payment_Approved),
        PaymentStatus.CANCELED => nameof(EventType.Payment_Cancelled),
        _ => nameof(EventType.Payment_Error)
    };

    public static string FromToken(string token)
    {
        var t = (token ?? string.Empty).Trim().ToUpperInvariant();
        return t switch
        {
            "CONSULTA" => nameof(EventType.Payment_Consulted),
            "ATUALIZACAO" => nameof(EventType.Payment_Updated),
            "NOT_FOUND" => nameof(EventType.Payment_Not_Found),
            "PURCHASE_BY_USER" => nameof(EventType.Payment_Purchase_By_User),
            "PURCHASE_NOT_FOUND" => nameof(EventType.Payment_Purchase_Not_Found),
            "CANCELED" or "CANCELLED" => nameof(EventType.Payment_Cancelled),
            _ => FromStatusString(t)
        };
    }
}

