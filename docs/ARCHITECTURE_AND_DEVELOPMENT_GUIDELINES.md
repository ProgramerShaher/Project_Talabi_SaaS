# 🏛️ دليل المعمارية وقواعد العمل القياسية لنظام "طلبي" (Talabi SaaS)
### مبني وفق المعايير الرسمية لـ ABP Framework (Domain-Driven Design & Clean Modular Architecture)

---

## 🎯 الغرض من هذا الدليل
هذا المستند يمثل **الدستور البرمجي الموحد (Development Blueprint)** لمشروع **Talabi SaaS**.
أي مطور ينضم إلى فريق العمل يجب أن يقرأ هذا الدليل ليتبع نفس القواعد المعمارية بدقة متناهية، وبدون أي اجتهادات شخصية تخالف المعايير الرسمية المعتمدة لمطوري **ABP Framework**.

---

## 📐 1. الهيكل المعماري والطبقات (The Architectural Layers)

المشروع مبني على معمارية **Domain-Driven Design (DDD)** المقسمة طبقياً إلى 5 مشاريع رئيسية. يُمنع منعاً باتاً خلط المسؤوليات بين هذه الطبقات:

```
src/
├── 1. Talabi.Domain.Shared/            <-- الثوابت، الـ Enums، ملفات الترجمة، رموز الأخطاء
├── 2. Talabi.Domain/                   <-- الكيانات (Entities)، قواعد العمل الصافية (Domain Services)
├── 3. Talabi.EntityFrameworkCore/      <-- قاعدة البيانات، التهيئة (Fluent API)، الـ Migrations
├── 4. Talabi.Application.Contracts/    <-- الـ DTOs، الواجهات (Interfaces)، الصلاحيات (Permissions)
├── 5. Talabi.Application/              <-- خدمات التطبيق (App Services)، الـ Mapping، الـ Validation
└── 6. Talabi.HttpApi.Host/             <-- نقطة تشغيل السيرفر (API Host)، الإعدادات، Swagger
```

### 📋 مصفوفة المسؤوليات الصارمة لكل طبقة:

| الطبقة | ماذا نضع بداخلها؟ | الممنوعات الصارمة 🚫 |
| :--- | :--- | :--- |
| **`Domain.Shared`** | • Enums مشتركة<br>• ثوابت أطوال الحقول (`ProductConsts`)<br>• ملفات الترجمة `ar.json` و `en.json`<br>• رموز الأخطاء (Error Codes) | ❌ ممنوع أي اعتماد (Dependency) على أي مشروع آخر داخل الـ Solution. |
| **`Domain`** | • الكيانات (`Entities` & `AggregateRoots`)<br>• الـ `ValueObjects`<br>• خدمات النطاق (`Domain Services / Managers`)<br>• واجهات المستودعات المخصصة (`IRepositories`) | ❌ ممنوع استخدام الـ DTOs نهائياً.<br>❌ ممنوع أي كود يتعلق بـ EF Core أو HTTP أو UI. |
| **`EntityFrameworkCore`** | • كلاسات تهيئة الجداول (`IEntityTypeConfiguration<T>`)<br>• الـ `TalabiDbContext`<br>• الـ Migrations وعمليات Seed Data | ❌ ممنوع كتابة أي Business Logic هنا إطلاقاً. |
| **`Application.Contracts`** | • كائنات نقل البيانات (`DTOs`)<br>• واجهات الخدمات (`IApplicationServices`)<br>• تعريف الصلاحيات (`Permissions`)<br>• كلاسات الفلترة والترقيم الخاصة بالـ APIs | ❌ ممنوع وجود أي كود تنفيذي (Implementation). فقط Contracts مجردة. |
| **`Application`** | • تنفيذ الخدمات (`Application Services`)<br>• الـ AutoMapper Profiles<br>• التحقق من البيانات (Fluent Validation)<br>• استدعاء الـ Domain Services والـ Repositories | ❌ ممنوع إرجاع الكيان (Entity) خارج هذه الطبقة إطلاقاً؛ الإرجاع دائماً يكون DTO. |

---

## 📂 2. استراتيجية تنظيم الميزات (Feature-Driven Folder Structure)

لكل ميزة (مثلاً ميزة المنتجات `Products`)، يتم تقسيم ملفاتها بشكل منظم ونظيف داخل كل طبقة في مجلد يحمل اسم الميزة:

```text
src/
│
├── 📂 Talabi.Domain.Shared/
│   └── 📂 Products/
│       ├── ProductConsts.cs                 <-- ثوابت أطوال الأسماء والرموز
│       └── ProductType.cs                   <-- Enum نوع المنتج إن وجد
│
├── 📂 Talabi.Domain/
│   └── 📂 Products/
│       ├── Product.cs                       <-- الكيان (Entity / AggregateRoot)
│       └── ProductManager.cs                <-- Domain Service لقواعد العمل المعقدة
│
├── 📂 Talabi.EntityFrameworkCore/
│   └── 📂 EntityConfigurations/
│       └── Products/
│           └── ProductConfiguration.cs      <-- تهيئة الجدول بـ Fluent API
│
├── 📂 Talabi.Application.Contracts/
│   └── 📂 Products/
│       ├── 📂 Dtos/
│       │   ├── ProductDto.cs                <-- كائن العرض للفرونت إند
│       │   ├── CreateProductDto.cs          <-- كائن إضافة منتج جديد
│       │   ├── UpdateProductDto.cs          <-- كائن تعديل منتج
│       │   └── GetProductListInput.cs       <-- كائن الترقيم والفلترة والترتيب
│       └── IProductAppService.cs            <-- واجهة السيرفس
│
└── 📂 Talabi.Application/
    └── 📂 Products/
        ├── ProductAppService.cs             <-- تنفيذ الـ AppService وتوليد الـ API
        ├── ProductAutoMapperProfile.cs      <-- ملف الـ Mapping الصريح
        └── ProductValidator.cs              <-- قواعد التحقق المتقدمة (FluentValidation)
```

> ⚠️ **قاعدة ذهبية:** كل ملف يمثل كلاس واحد فقط؛ لا تضع أكثر من DTO أو Interface في نفس الملف!

---

### 🗺️ جدول قرار المطور: "أين أضع كودي بالضبط؟" (Developer Decision Matrix)

إذا كنت محتاراً أين تضع أي كود جديد، استعن بهذا الجدول المباشر:

| ما الذي تريد كتابته أو إضافته؟ | المشروع المستهدف | المسار والمجلد النموذجي |
| :--- | :--- | :--- |
| **جدول جديد في قاعدة البيانات** | `Talabi.Domain` | `📂 Products/Product.cs` |
| **أقصى طول لنص أو قيم Enum** | `Talabi.Domain.Shared` | `📂 Products/ProductConsts.cs` |
| **قواعد Fluent API وفهارس الجدول** | `Talabi.EntityFrameworkCore` | `📂 EntityConfigurations/Products/ProductConfiguration.cs` |
| **كائن استقبال بيانات (Input)** | `Talabi.Application.Contracts` | `📂 Products/Dtos/CreateProductDto.cs` |
| **كائن إرجاع بيانات (Output)** | `Talabi.Application.Contracts` | `📂 Products/Dtos/ProductDto.cs` |
| **كائن طلب فلترة وبحث وترقيم** | `Talabi.Application.Contracts` | `📂 Products/Dtos/GetProductListInput.cs` |
| **واجهة الخدمة (Interface)** | `Talabi.Application.Contracts` | `📂 Products/IProductAppService.cs` |
| **صلاحيات جديدة للميزة (Permissions)** | `Talabi.Application.Contracts` | `📂 Permissions/TalabiPermissions.cs` |
| **كود العمليات والـ CRUD الفعلي** | `Talabi.Application` | `📂 Products/ProductAppService.cs` |
| **قواعد المابينج بين الكيان والـ DTO** | `Talabi.Application` | `📂 Products/ProductAutoMapperProfile.cs` |
| **فحص منطق بيزنس معقد يمس عدة جداول**| `Talabi.Domain` | `📂 Products/ProductManager.cs` |
| **ترجمة نص أو رسالة خطأ** | `Talabi.Domain.Shared` | `📂 Localization/Talabi/ar.json` |

---

### 🔄 مسار تدفق البيانات من الطلب حتى قاعدة البيانات (Request-Response Lifecycle):

```text
[ Front-End / Client (Angular / React / Mobile) ]
       │
       ▼  (1) إرسال HTTP Request مع DTO المدخلات والـ JWT Token
[ Talabi.HttpApi.Host / Swagger ]
       │
       ▼  (2) توجيه الطلب تلقائياً عبر Auto API Controllers
[ Talabi.Application / ProductAppService ]
       │  ├─ فحص الصلاحيات [Authorize]
       │  ├─ التحقق من المدخلات (FluentValidation)
       │  ▼
       ├──► إذا كان هناك منطق أعمال معقد ──► [ Talabi.Domain / ProductManager ]
       │                                            │
       │  (3) استدعاء المستودع IRepository          │
       ▼                                            ▼
[ Talabi.EntityFrameworkCore / Repositories ]
       │
       ▼  (4) تنفيذ استعلام SQL المفلتر تلقائياً (TenantId + SoftDelete)
[ Database (SQL Server / PostgreSQL) ]
       │
       ▼  (5) استرجاع الـ Entity
[ Talabi.Application / ProductAppService ]
       │
       ▼  (6) تحويل Entity إلى DTO عبر AutoMapper (لا تخرج Entity أبداً)
[ Front-End / Client ]  ◄─── (7) استلام PagedResultDto<ProductDto>
```

---

## 🔄 3. دورة حياة تنفيذ ميزة جديدة (Step-by-Step Workflow)

عندما يُطلب منك بناء ميزة جديدة، اتبع الترتيب التالي بدقة:

1. **الخطوة 1: `Domain.Shared`**: تعريف الثوابت (أقصى طول للحقول) والـ Enums.
2. **الخطوة 2: `Domain`**: بناء الكيان `Entity` والوراثة من الكلاس المناسب (`FullAuditedAggregateRoot<Guid>`).
3. **الخطوة 3: `EntityFrameworkCore`**: إنشاء كلاس `Configuration` للجدول باستخدام Fluent API وإضافته للـ `DbContext` ثم عمل Migration.
4. **الخطوة 4: `Application.Contracts`**: إنشاء كلاسات الـ DTOs وواجهة الـ `IAppService` وصلاحيات الميزة.
5. **الخطوة 5: `Application`**: إعداد الـ AutoMapper، كتابة الـ `AppService`، والتحقق من البيانات.
6. **الخطوة 6: التوثيق والتسجيل**: إضافة تعليقات `/// <summary>` بالعربية وتسجيل الخطوات بـ Serilog.

---

## 📦 4. نظام الـ DTOs والتحقق الصارم من البيانات (Zero Entity Exposure)

### ⛔ حظر التعامل المباشر مع الـ Entities:
ممنوع نهائياً استقبال أو إرجاع أي Entity من الـ APIs؛ كل المدخلات والمخرجات تمر عبر DTOs مخصصة ومفصولة:

1. **`ProductDto`**: لعرض البيانات (يرث من `AuditedEntityDto<Guid>`).
2. **`CreateProductDto`**: لإنشاء عنصر جديد (يحتوي فقط على الحقول المطلوبة للإنشاء).
3. **`UpdateProductDto`**: لتعديل عنصر موجود.
4. **`GetProductListInput`**: للفلترة والترتيب والترقيم.

### 🛡️ التحقق المزدوج من البيانات (Data Annotations + FluentValidation):
* **في كلاس الـ DTO:** نستخدم Data Annotations للتحقق الأساسي من الحقول والأطوال:
  ```csharp
  public class CreateProductDto
  {
      /// <summary>
      /// اسم المنتج
      /// </summary>
      [Required(ErrorMessage = "اسم المنتج مطلوب")]
      [StringLength(ProductConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى المسموح لاسم المنتج")]
      public string Name { get; set; } = string.Empty;

      /// <summary>
      /// سعر بيع المنتج
      /// </summary>
      [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
      public decimal Price { get; set; }
  }
  ```
* **في طبقة `Application`:** نستخدم FluentValidation لقواعد الأعمال والتحقق المتقدم (مثل التأكد من الشروط المركبة أو فحص التكرار).

---

## 📑 5. المعيار الموحد للترقيم والفلترة والترتيب (Unified Pagination & Filtering)

### 1. كائن الطلب القياسي (Request Contract):
كل شاشة تحتوي على جدول أو قائمة ترث من `PagedAndSortedResultRequestDto`:

```csharp
using Volo.Abp.Application.Dtos;

namespace Talabi.Products.Dtos;

/// <summary>
/// معايير طلب قائمة المنتجات مع الترقيم والفلترة والترتيب
/// </summary>
public class GetProductListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// كلمة البحث العامة (في الاسم أو الكود أو الباركود)
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// فلتر بالحد الأدنى للسعر
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// فلتر بالحد الأقصى للسعر
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// فلتر بمعرف التصنيف
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// فلتر بحالة التفعيل
    /// </summary>
    public bool? IsActive { get; set; }
}
```

### 2. كائن الاستجابة القياسي (Response Contract):
النتيجة تعود دائماً داخل كائن ABP الموحد: `PagedResultDto<ProductDto>`:
* **`TotalCount`** (`long`): إجمالي عدد السجلات المطابقة للفلتر في قاعدة البيانات.
* **`Items`** (`IReadOnlyList<ProductDto>`): مصفوفة العناصر المقتطعة للصفحة الحالية.

### 3. التطبيق القياسي في الـ Service باستخدام `WhereIf` و `PageBy`:
```csharp
public async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListInput input)
{
    // 1. إنشاء الـ Query الأساسي
    var queryable = await _productRepository.GetQueryableAsync();

    // 2. تطبيق الفلاتر الديناميكية الموحدة
    var filteredQuery = queryable
        .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!))
        .WhereIf(input.MinPrice.HasValue, x => x.Price >= input.MinPrice!.Value)
        .WhereIf(input.MaxPrice.HasValue, x => x.Price <= input.MaxPrice!.Value)
        .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId!.Value)
        .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

    // 3. حساب العدد الكلي بعد الفلترة
    var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

    // 4. تطبيق الترتيب والترقيم
    var items = await AsyncExecuter.ToListAsync(
        filteredQuery
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Product.CreationTime) + " DESC" : input.Sorting)
            .PageBy(input)
    );

    // 5. التحويل إلى DTO والإرجاع
    var dtos = ObjectMapper.Map<List<Product>, List<ProductDto>>(items);
    return new PagedResultDto<ProductDto>(totalCount, dtos);
}
```

---

## 🚦 6. معالجة الاستثناءات والاستجابة الموحدة للأخطاء (Error Handling)

### ⛔ قاعدة صارمة: لا تكتب `try-catch` عامة في الـ AppServices!
يتولى ABP Framework معالجة الأخطاء تلقائياً عبر الـ Exception Filters المدمجة وتحويلها إلى استجابة موحدة `RemoteServiceErrorResponse`.

### كيف ترمي خطأ بيزنس للمستخدم؟
1. **أخطاء الأعمال المباشرة:** نستخدم `UserFriendlyException`:
   ```csharp
   if (product.StockQuantity < input.Quantity)
   {
       throw new UserFriendlyException(L["InsufficientStock", product.Name]);
   }
   ```
2. **أخطاء الأعمال المعرفة بأكواد (Domain Business Exceptions):**
   ```csharp
   throw new BusinessException(TalabiDomainErrorCodes.ProductAlreadyExists)
       .WithData("Name", input.Name);
   ```

### 📄 شكل الاستجابة عند النجاح (Success Response):
في إطار عمل ABP (والمعايير العالمية للـ RESTful APIs)، يعبر كود حالة الـ HTTP عن نجاح العملية (مثلاً `200 OK` أو `201 Created`).
لا يتم تغليف البيانات الناجحة بكلمات مثل "success: true"، بل يتم **إرجاع البيانات الفعلية مباشرة** لتسهيل عمل مطور الفرونت إند وتقليل حجم البيانات (Payload).
مثال لنتيجة إضافة أو جلب منتج بنجاح:
```json
{
  "id": "3a0937d2-7c8a-a43b-3d6f-8a176b9f2b8a",
  "storeId": "2b9215c1-1c5a-a32b-9d4f-1a276b9f1b7a",
  "storeCategoryId": "5c9215c1-1c5a-a32b-9d4f-1a276b9f1b7c",
  "name": "آيفون 15 برو",
  "price": 4500,
  "isAvailable": true,
  "isActive": true
}
```

### 📄 شكل الاستجابة الموحدة عند الفشل والأخطاء (Error Response):
إذا تم رمي `UserFriendlyException` أو حدث خطأ في النظام أو فشل في الـ Validation، سيعود كود حالة HTTP مثل `400 Bad Request` أو `403 Forbidden`، مع هذا القالب الثابت والموحد:
```json
{
  "error": {
    "code": "Talabi:00102",
    "message": "عفواً، الكمية المطلوبة غير متوفرة في المخزون للمنتج (آيفون 15)!",
    "details": null,
    "validationErrors": null
  }
}
```
* **ملحوظة للفرونت إند:** يمكنك دائماً قراءة `error.message` لعرض الرسالة الصريحة للمستخدم (مثل: "اسم المنتج مطلوب"، "رقم الجوال مسجل مسبقاً"، إلخ).

---

## 🗺️ 7. قواعد الـ AutoMapper الصريحة (Explicit Object Mapping)

* **ممنوع التخمين أو المابينج الغامض:**
* لكل ميزة ملف Mapping خاص بها في مجلد الميزة: `ProductAutoMapperProfile.cs`:
```csharp
using AutoMapper;
using Talabi.Products.Dtos;

namespace Talabi.Products;

/// <summary>
/// ملف تعريف تحويل الكائنات الخاص بالمنتجات
/// </summary>
public class ProductAutoMapperProfile : Profile
{
    public ProductAutoMapperProfile()
    {
        // من الكيان إلى كائن العرض
        CreateMap<Product, ProductDto>();

        // من كائن الإضافة إلى الكيان (تجاهل المعرف وحقول التدقيق لتوليدها تلقائياً)
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore());

        // من كائن التعديل إلى الكيان
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore());
    }
}
```

---

## ⚙️ 8. معايير تهيئة الجداول بـ Entity Framework Core (Fluent API)

* **لا تستخدم Data Annotations على الـ Entities** (مثل `[Table]`, `[MaxLength]`).
* كل جدول يُهيأ في كلاس مستقل يرث من `IEntityTypeConfiguration<T>` داخل `Talabi.EntityFrameworkCore`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabi.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Talabi.EntityConfigurations.Products;

/// <summary>
/// إعدادات جدول المنتجات وقيود قاعدة البيانات
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 1. تطبيق إعدادات ABP الأساسية (Auditing, SoftDelete, MultiTenancy)
        builder.ConfigureByConvention();

        // 2. اسم الجدول والـ Schema
        builder.ToTable("AppProducts");

        // 3. خصائص الحقول والقيود
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProductConsts.MaxNameLength);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(ProductConsts.MaxCodeLength);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        // 4. الفهارس (Indexes) لتحسين سرعة البحث
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => x.Name);
    }
}
```

---

## 📝 9. استراتيجية التسجيل والتعقب المتقدمة (Structured Logging with Serilog)

يوفر مشروعك مكتبة **Serilog** مهيأة وجاهزة. استخدم خاصية `Logger` الموروثة من `ApplicationService` لتسجيل الأحداث والعمليات:

### مستويات التسجيل المعتمدة:
1. **`LogInformation`**: للعمليات الحيوية الناجحة (إنشاء طلب، إتمام دفع، تغيير حالة مستأجر).
2. **`LogWarning`**: عند حدوث شيء غير متوقع يمكن للنظام التعامل معه (محاولة إدخال كود خصم منتهي الصلاحية).
3. **`LogError`**: عند حدوث فشل غير متوقع أو استثناء تقني حرج.

### 💡 الطريقة الاحترافية (Structured Logging):
سجل المتغيرات داخل نصوص التسجيل ككائنات منظمة:
```csharp
// ✅ الطريقة الصحيحة - Structured Logging:
Logger.LogInformation(
    "تم إنشاء المنتج بنجاح: {ProductName} برقم معرف {ProductId} للمتجر {TenantId}", 
    product.Name, 
    product.Id, 
    CurrentTenant.Id
);

// ❌ الطريقة الخاطئة (لا تستخدم دمج النصوص String Concatenation):
// Logger.LogInformation("Product " + product.Name + " created.");
```

---

## ✍️ 10. معايير التوثيق بالـ XML Summary (باللغة العربية)

لكي تظهر تفاصيل أي كلاس أو دالة أو خاصية عند تمرير الماوس فوقها (Hover / IntelliSense)، يجب توثيق كل العناصر بـ `/// <summary>` باللغة العربية الواضحة:

```csharp
/// <summary>
/// خدمة إدارة المنتجات - مسؤولة عن عمليات الإضافة والتعديل والفلترة
/// </summary>
public class ProductAppService : ApplicationService, IProductAppService
{
    /// <summary>
    /// إضافة منتج جديد إلى متجر المستأجر الحالي
    /// </summary>
    /// <param name="input">بيانات إنشاء المنتج المطلوبة</param>
    /// <returns>كائن المنتج بعد الإنشاء شاملاً المعرف وتاريخ الإنشاء</returns>
    /// <exception cref="UserFriendlyException">تُرمى في حال كان اسم المنتج مكرراً</exception>
    [Authorize(TalabiPermissions.Products.Create)]
    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        // ...
    }
}
```

---

## 🌟 11. استغلال ميزات ABP الجاهزة (Do Not Reinvent the Wheel)

تأكد دائماً قبل كتابة أي كود مخصص من استخدام ميزات ABP الجاهزة:

1. **`CurrentTenant`**: لمعرفة المستأجر الحالي وعزله تلقائياً.
2. **`CurrentUser`**: لمعرفة المستخدم الحالي (`Id`, `UserName`, `Email`, `Roles`).
3. **`ISoftDelete`**: الحذف الآمن دون فقد البيانات.
4. **`Clock`**: استخدام `Clock.Now` دائماً بدلاً من `DateTime.Now` لضمان توافق المناطق الزمنية (Timezones).
5. **`GuidGenerator`**: استخدام `GuidGenerator.Create()` لتوليد GUIDs متسلسلة ومتوافقة مع كفاءة الفهارس في SQL Server بدلاً من `Guid.NewGuid()`.
6. **`L["Key"]`**: استخدام الترجمة الموحدة بدلاً من النصوص الثابتة (Hardcoded Strings).

---

## 🧱 12. الالتزام الصارم بمبادئ SOLID البرمجية (SOLID Principles in Action)

يجب أن يعكس كل سطر كود في المشروع المبادئ الخمسة للتصميم الكائني النظيف (SOLID) بطريقة عملية:

### 1. مبدأ المسؤولية الواحدة (Single Responsibility Principle - SRP):
* **لكل كلاس سبب واحد فقط للتغيير:**
  - **الـ Entity:** مسؤول فقط عن تمثيل بياناته وحماية قواعده الداخلية (Invariants).
  - **الـ Domain Service (Manager):** مسؤول فقط عن قواعد العمل المعقدة التي تحتاج لفحص أكثر من كيان (مثل التحقق من تكرار كود المنتج في قاعدة البيانات).
  - **الـ AppService:** مسؤول فقط عن التنسيق (Orchestration) وتفويض المهام (تلقي DTO ⬅ استدعاء Manager/Repository ⬅ عمل Mapping ⬅ إرجاع النتيجة)، ولا يكتب فيه كود SQL أو قواعد بيزنس معقدة.
  - **الـ Configuration:** مسؤول فقط عن رسم شكل الجدول في قاعدة البيانات.
  - **الـ Validator:** مسؤول فقط عن التحقق من صحة المدخلات.

---

### 2. مبدأ الفتح والإغلاق (Open/Closed Principle - OCP):
* **الكود مفتوح للتوسع (Extension)، ومغلق أمام التعديل (Modification):**
  - بدلاً من التعديل في كود دالة إنشاء الطلب `CreateOrderAsync` لإضافة (إرسال إيميل، أو إشعار واتساب، أو خصم نقاط الولاء)، نستخدم **نظام الأحداث الموزعة (ABP Event Bus)**:
  ```csharp
  // في الـ AppService: نطلق الحدث فقط دون معرفة من سيستقبله
  await _localEventBus.PublishAsync(new OrderCreatedEvent(order.Id));
  ```
  - ثم يُنشأ كلاس مستقل `SendEmailOnOrderCreatedHandler` يستمع للحدث. مستقبلاً، إذا أردت إضافة إشعار SMS، تنشئ Handler جديداً تماماً **دون لمس أو تعديل سطر واحد** في خدمة الطلبات الأصلية.

---

### 3. مبدأ استبدال لسكوف (Liskov Substitution Principle - LSP):
* **أي كلاس مشتق يجب أن يكون قادراً على الحلول محل كلاس الأصل دون كسر سلوك البرنامج:**
  - عند التوريث من كلاسات ABP الجاهزة مثل `CrudAppService<...>`، يُمنع منعاً باتاً رمي استثناء `NotImplementedException` في أي دالة موروثة. إذا كانت الميزة لا تدعم الحذف مثلاً، نقوم بإنشاء خدمة مخصصة `ApplicationService` تطبق واجهة بدون دالة الحذف بدلاً من تخريب كلاس `CrudAppService`.

---

### 4. مبدأ فصل الواجهات (Interface Segregation Principle - ISP):
* **لا تجبر أي كلاس على الاعتماد على واجهة تحوي دوالاً لا يحتاجها:**
  - يلتزم المشروع بفلسفة واجهات ABP الدقيقة والمفصولة:
    - الكيان الذي يحتاج تدقيق إنشاء فقط يطبق: `IHasCreationTime`.
    - الكيان الذي يحتاج تدقيقاً كاملاً يطبق: `IFullAuditedObject`.
    - الكيان التابع لمتجر يطبق: `IMultiTenant`.
  - **في واجهات الـ Services:** لا تنشئ واجهة ضخمة واحدة اسمها `IStoreService` تحتوي على 50 دالة لكل شيء؛ بل نقسمها حسب الوظيفة:
    - `IProductAppService`: خاصة بالمنتجات فقط.
    - `ICategoryAppService`: خاصة بالتصنيفات فقط.
    - `IProductInventoryAppService`: خاصة بعمليات الجرد والمخزون فقط.

---

### 5. مبدأ عكس التبعيات (Dependency Inversion Principle - DIP):
* **الوحدات عالية المستوى لا تعتمد على وحدات منخفضة المستوى؛ كلاهما يعتمد على التجريدات (Abstractions):**
  - طبقة الـ `Domain` والـ `Application` لا تعتمد على `TalabiDbContext` أو SQL Server بشكل مباشر، بل تعتمد على الواجهات المجردة:
    `IRepository<Product, Guid>` أو `IProductRepository`.
  - **الحقن عبر الباني (Constructor Injection) دائماً:** ممنوع استخدام `new` لإنشاء الخدمات، بل تُحقن الواجهات عبر الـ Dependency Injection (Autofac/ABP DI) مع تحديد دورة الحياة المناسبة (`ITransientDependency` أو `ISingletonDependency`).

---

## 🚫 13. مبدأ عدم التكرار (DRY) وحظر إعادة اختراع العجلة (Zero-Reinvention & Anti-Duplication Policy)

يُحظر في هذا المشروع كتابة كود مخصص لأي ميزة يقدمها **ABP Framework** مسبقاً، ويجب الالتزام التام بقاعدة **DRY (Don't Repeat Yourself)**:

### 1. قائمة الممنوعات البرمجية الصارمة (ما يوفره ABP وممنوع إعادة برمجته):
| الميزة | ما يمنع فعله ❌ | البديل المعتمد من ABP الواجب استخدامه ✅ |
| :--- | :--- | :--- |
| **المستخدمين والصلاحيات** | إنشاء جداول `Users`, `Roles`, `Permissions` مخصصة | استخدام موديل **`Volo.Abp.Identity`** و **`PermissionDefinitionProvider`** الجاهز. |
| **تعدد الشركات (SaaS)** | إنشاء جدول `Tenants` أو فلترة `TenantId` يدوياً في كل كويري | تطبيق واجهة **`IMultiTenant`** وتفعيل فلتر العزل التلقائي. |
| **التدقيق والتتبع** | إضافة حقول `CreatedAt`, `CreatedBy` وكتابة وقت الحفظ يدوياً | توريث الكيان من **`FullAuditedAggregateRoot<TKey>`** لتسجيل كل شيء آلياً. |
| **الحذف الآمن** | إنشاء حقل `IsDeleted` وتعديل الاستعلامات يدوياً | تطبيق **`ISoftDelete`** وفلاتر البيانات التلقائية المدمجة. |
| **كتابة الـ Controllers** | إنشاء Controller لكل Entity لعمليات الـ CRUD العادية | استخدام ميزة **Auto API Controllers**؛ مجرد كتابة الـ `AppService` يولد الـ API تلقائياً. |
| **معالجة الـ Exceptions** | كتابة بلوكات `try-catch` و Middleware مخصص للأخطاء | الاعتماد على **`RemoteServiceErrorResponse`** ورمي **`UserFriendlyException`**. |
| **الترقيم والتقطيع** | كتابة معادلات `.Skip()` و `.Take()` يدوياً في كل استعلام | استخدام دالة **`.PageBy(input)`** وكلاسات **`PagedResultDto<T>`**. |
| **إدارة الإعدادات** | إنشاء جدول Settings وحفظ خيارات النظام يدوياً | استخدام **`ISettingManager`** و **`SettingDefinitionProvider`** المدمج. |
| **باقات الميزات (SaaS)** | فحص اشتراك العميل وما إذا كانت الميزة مفعلة يدوياً | استخدام نظام **`IFeatureChecker`** المدمج مع الـ Tenants. |
| **الذاكرة المؤقتة (Caching)** | استخدام `IMemoryCache` المباشر وكتابة كود الكاش يدوياً | استخدام **`IDistributedCache<TCacheItem>`** المدمج في ABP. |

---

### 2. محاربة تكرار الكود (Anti-Duplication) في العمليات اليومية:

#### أ) لا تكرر عمليات الـ CRUD القياسية:
إذا كانت الميزة تقتصر على عمليات إضافة/تعديل/حذف/جلب عادية، **يُمنع كتابة 5 دوال مكررة من الصفر**، بل نورث من كلاس ABP الجاهز:
```csharp
// سطر واحد يوفر عليك كتابة 100 سطر كود مكرر!
public class CategoryAppService : 
    CrudAppService<Category, CategoryDto, Guid, GetCategoryListInput, CreateUpdateCategoryDto>,
    ICategoryAppService
{
    public CategoryAppService(IRepository<Category, Guid> repository) : base(repository)
    {
    }
}
```

#### ب) استخدم دوال الاختصار الذكية بدلاً من تكرار الشروط:
* **بدل كتابة الشروط المكررة في LINQ:**
  ```csharp
  // ❌ تكرار غير احترافي:
  if (!string.IsNullOrWhiteSpace(input.Filter))
  {
      query = query.Where(x => x.Name.Contains(input.Filter));
  }

  // ✅ استخدام دالة ABP الجاهزة النظيفة:
  query = query.WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter));
  ```
* **بدل كتابة التحقق من القيم الفارغة (Null Checks):**
  ```csharp
  // ❌ تكرار يدوي:
  if (product == null) throw new EntityNotFoundException(...);

  // ✅ استخدام أداة فحص ABP الجاهزة:
  Check.NotNull(product, nameof(product));
  Check.NotNullOrWhiteSpace(input.Name, nameof(input.Name));
  ```

---

## 📁 14. معايير التعامل مع رفع الملفات والوسائط و `IFormFile` (File Upload Policy)

### ❓ لماذا لا نستخدم `IFormFile` داخل الكيانات (Entities)؟
* **قاعدة معمارية صارمة في Clean Architecture و DDD:**
  - طبقة الـ `Domain` والـ `Entities` تمثل نموذج العمل وقاعدة البيانات النقيّة، ويجب أن تكون مستقلة تماماً عن أي تقنية ويب أو حزم HTTP (`Microsoft.AspNetCore.Http`).
  - قاعدة البيانات لا تُخزّن ملفات خام (Binary Blobs) لما يسببه ذلك من تضخم وبطء في الاستعلامات؛ بل يتم رفع الملف إلى وسيط تخزين (Amazon S3, Cloudflare R2, MinIO, أو Local Disk).
  - في قاعدة البيانات (في جدول `MediaFiles`)، نُسجل فقط البيانات الوصفية للملف: (`FileName`, `ContentType`, `FileSize`, `PublicUrl`, `BucketName`, `ObjectKey`, `Hash`).

### 📍 أين وكيف يُستخدم `IFormFile` الصحيح؟
* يُستخدم `IFormFile` **فقط وحصرياً في كائنات الـ DTO للرفع** داخل طبقة `Application.Contracts` أو واجهات الـ `HttpApi`:
```csharp
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Talabi.MediaFiles.Dtos;

/// <summary>
/// كائن استقبال رفع ملف أو صورة جديدة من الفرونت إند
/// </summary>
public class UploadMediaFileInput
{
    /// <summary>
    /// الملف المرفوع عبر HTTP Request
    /// </summary>
    [Required(ErrorMessage = "الملف مطلوب للرفع")]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// نوع الكيان المرتبط (Product, Store, Receipt)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public Guid? EntityId { get; set; }
}
```

---

## 📑 15. معايير تنظيم الكود الداخلي باستخدام `#region` (Code Organization with Regions)

لضمان سهولة قراءة الكود، وتسهيل تصفحه وطي أجزائه (Collapsing / Expanding) داخل Visual Studio، **يلتزم الفريق بتنظيم محتويات الكلاسات باستخدام الـ `#region` كمعيار إلزامي**:

### الهيكل القياسي الموحد للكلاسات:

```csharp
public class SampleAppService : ApplicationService
{
    #region 1. Fields & Dependencies (الحقول والتبعيات المحقونة)
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IProductManager _productManager;
    #endregion

    #region 2. Constructors (البواني)
    public SampleAppService(
        IRepository<Product, Guid> productRepository,
        IProductManager productManager)
    {
        _productRepository = productRepository;
        _productManager = productManager;
    }
    #endregion

    #region 3. Public Methods / Actions (العمليات العامة)
    public async Task<ProductDto> GetAsync(Guid id)
    {
        // ...
    }
    #endregion

    #region 4. Private Helper Methods (الدوال المساعدة الخاصة)
    private void ValidateBusinessRules(...)
    {
        // ...
    }
    #endregion
}
```

### الهيكل القياسي الموحد للكيانات (Entities):
```csharp
public class SampleEntity : FullAuditedAggregateRoot<Guid>
{
    #region 1. Properties (خصائص وحقول البيانات)
    public virtual string Name { get; set; } = string.Empty;
    public virtual decimal Price { get; set; }
    #endregion

    #region 2. Navigation Properties & Relations (علاقات الكيان)
    public virtual ICollection<SampleItem> Items { get; protected set; } = new List<SampleItem>();
    #endregion

    #region 3. Constructors (البواني)
    protected SampleEntity() { }
    public SampleEntity(Guid id, string name) : base(id) { Name = name; }
    #endregion

    #region 4. Business Logic Methods (دوال منطق العمل وحماية الحالة)
    public void UpdatePrice(decimal newPrice) { ... }
    #endregion
}
```

---

## 🏬 16. ضوابط نطاق العمل وحدود النظام (Talabi SaaS Scope & Business Boundaries)

بناءً على التوجه الاستراتيجي المعتمد لمنصة **طلبي** لتبسيط العمليات وتقديم تجربة استخدام خفيفة وسريعة للمتاجر والعملاء:

### 1. إلغاء إدارة المخزون وتتبع الكميات (No Inventory Tracking Policy)
* **المبدأ:** المنصة منصة عرض وطلب فوري؛ لا تتدخل إطلاقاً في إدارة المستودعات أو تعداد المخزون أو حجز الكميات.
* **التطبيق:** تم تعطيل كيان `Inventory` بالكامل في كافة الطبقات (Domain, EF Core, DTOs, Validation).
* **إدارة التوفر:** يكتفي التاجر بتحديد ما إذا كان الصنف متاحاً للطلب حالياً عبر `IsAvailable`، ومفعلاً بالمنصة عبر `IsActive`.

### 2. تبسيط بيانات وخصائص المنتج (Simplified Product Model)
* تم تعطيل الحقول الثانوية التي تثقل على التجار إدخالها وتخرج عن نطاق العرض:
  - `Barcode` (الباركود الدولي)
  - `CostPrice` (سعر التكلفة الداخلي للتاجر)
  - `Brand` (الماركة التجارية)
  - `Weight` و `WeightUnit` (الوزن ووحدته)
  - `Origin` (بلد المنشأ)
* **التركيز الأساسي:** اسم المنتج، التصنيف، السعر، الخصم ونوعه، الوصف، وحدة البيع، والصور المميزة.

### 3. استقلالية المتاجر في التوصيل (Store-Managed Delivery & Zero Platform Interference)
* **المبدأ:** المتجر هو المسؤول الأول والأخير عن آلية وتكاليف التوصيل لعملائه.
* **التطبيق:** المنصة لا تفرض أو تحدد رسوم توصيل `DeliveryFee` ولا تتدخل في حساب أو توزيع أرباح المشاوير `CourierEarning` و `TotalEarnings`.

### 4. التقييم المباشر للنشاط التجاري (Store-Centric Reviews)
* **المبدأ:** التقييم يمثل سمعة المتجر كنشاط تجاري متكامل وليس تقييماً فردياً لكل مشوار طلب.
* **التطبيق:** تم فك ارتباط التقييم بـ `OrderId` و `CourierId`؛ وأصبح التقييم موجهاً مباشرة للمتجر `StoreId`، مع تمكين العميل من تعديل تقييمه أو إضافة تقييم جديد للنشاط التجاري.

---

## 📱 17. استراتيجية واجهة المستخدم والمنصات المستهدفة (Frontend & PWA Strategy)

بناءً على التوجه الحديث لمنصة "طلبي" وتجربة المستخدم المطلوبة، لن يتم تطوير تطبيق موبايل "Native" منفصل، بل سيتم اعتماد تقنية تطبيق الويب التقدمي (Progressive Web App - PWA).

### 1. تطبيق ويب تقدمي (PWA) كبديل للتطبيقات التقليدية
* **المبدأ:** يتم تطوير واجهة الويب الأمامية (Frontend) بحيث تكون متجاوبة بالكامل (Fully Responsive) وتدعم معايير PWA.
* **التجربة:** يمكن للعملاء، التجار، ومندوبي التوصيل تثبيت التطبيق مباشرة من المتصفح كـ "تطبيق مستقل" (Standalone App) يظهر في الشاشة الرئيسية (Home Screen) لأجهزة الأندرويد، الآيفون (iOS)، وسطح المكتب (Desktop) بدون الحاجة للمرور بمتاجر التطبيقات (App Store / Google Play).

### 2. ميزات التوجه نحو PWA في المنصة
* **سرعة التحديث وإطلاق الميزات:** التحديثات تصل فوراً لجميع المستخدمين بمجرد نشرها على السيرفر بدون انتظار موافقات مراجعة المتاجر.
* **التوافق التام:** كود (Codebase) واجهة واحد يخدم جميع الشاشات (Desktop, Tablet, Mobile).
* **تجربة الموبايل (Native-like Experience):** التطبيق سيعمل بملء الشاشة (Full Screen) بدون أشرطة المتصفح، ويدعم الإشعارات (Push Notifications).

---

> 📌 **ملاحظة للفريق:**
> هذا الدليل مرجع إلزامي لكل أعضاء الفريق. أي Pull Request أو كود جديد يجب أن يلتزم بهذه البنود بنسبة 100% لضمان أعلى مستويات الجودة والأداء وسهولة التوسع المستقبلي.


