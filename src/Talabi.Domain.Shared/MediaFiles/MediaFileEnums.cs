namespace Talabi.MediaFiles;

/// <summary>
/// مستوى الوصول للملف أو الصورة
/// </summary>
public enum MediaAccessLevel
{
    /// <summary>
    /// عام - متاح للجميع
    /// </summary>
    Public = 0,

    /// <summary>
    /// يحتاج تسجيل دخول
    /// </summary>
    Auth = 1,

    /// <summary>
    /// خاص ومحمي تماماً
    /// </summary>
    Private = 2
}

/// <summary>
/// حالة معالجة الملف
/// </summary>
public enum MediaProcessingStatus
{
    /// <summary>
    /// قيد الانتظار
    /// </summary>
    Pending = 0,

    /// <summary>
    /// تم الرفع بنجاح
    /// </summary>
    Uploaded = 1,

    /// <summary>
    /// تمت المعالجة وتوليد الصور المصغرة
    /// </summary>
    Processed = 2,

    /// <summary>
    /// فشلت عملية المعالجة
    /// </summary>
    Failed = 3
}
