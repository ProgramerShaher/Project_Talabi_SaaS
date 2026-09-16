using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Deliveries.Dtos;

namespace Talabi.Deliveries;

/// <summary>
/// محول الكائنات التلقائي لبيانات المناديب وعمليات التوصيل عبر Mapperly
/// </summary>
[Mapper]
public partial class DeliveryMapper
{
    /// <summary>
    /// تحويل كيان المندوب إلى كائن عرض CourierDto
    /// </summary>
    public partial CourierDto ToCourierDto(Courier source);

    /// <summary>
    /// تحويل قائمة من كيانات المناديب إلى قائمة CourierDto
    /// </summary>
    public partial List<CourierDto> ToCourierDtoList(List<Courier> source);

    /// <summary>
    /// تحويل كيان تعيين التوصيل إلى كائن عرض DeliveryAssignmentDto
    /// </summary>
    [MapperIgnoreTarget(nameof(DeliveryAssignmentDto.CourierName))]
    public partial DeliveryAssignmentDto ToDeliveryAssignmentDto(DeliveryAssignment source);

    /// <summary>
    /// تحويل قائمة تعيينات التوصيل إلى قائمة DeliveryAssignmentDto
    /// </summary>
    [MapperIgnoreTarget(nameof(DeliveryAssignmentDto.CourierName))]
    public partial List<DeliveryAssignmentDto> ToDeliveryAssignmentDtoList(List<DeliveryAssignment> source);

    /// <summary>
    /// تحويل كيان إثبات التسليم إلى كائن عرض DeliveryConfirmationDto
    /// </summary>
    public partial DeliveryConfirmationDto ToDeliveryConfirmationDto(DeliveryConfirmation source);
}
