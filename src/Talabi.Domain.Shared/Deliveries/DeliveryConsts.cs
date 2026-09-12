namespace Talabi.Deliveries;

/// <summary>
/// ثوابت وقيود حقول المندوبين
/// </summary>
public static class CourierConsts
{
    public const int MaxVehicleTypeLength = 50;
    public const int MaxVehicleNumberLength = 50;
    public const int MaxLicenseNumberLength = 50;
}

/// <summary>
/// ثوابت وقيود حقول تأكيدات التسليم
/// </summary>
public static class DeliveryConfirmationConsts
{
    public const int MaxVerificationCodeLength = 10;
    public const int MaxSignatureUrlLength = 500;
    public const int MaxPhotoProofUrlLength = 500;
    public const int MaxNotesLength = 500;
}
