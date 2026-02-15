# AI_PROMPTS.md

## เครื่องมือที่ใช้
- Claude — หลักๆ ใช้คุยเรื่อง architecture, ให้ generate โค้ด, และถกเรื่อง trade-off

---

## วิธีที่ใช้จริงๆ

ไม่ได้แค่พิมพ์ "เขียนโค้ดให้หน่อย" แล้วเอามาใช้เลย วิธีที่ใช้จริงคือ อธิบาย problem ให้ Claude ฟัง → Claude เสนอตัวเลือก → เราเลือกและบอกเหตุผล → Claude ช่วย implement

---

## Prompt 1 — ออกแบบ Tenant Isolation

**ที่ถาม:**
> "Multi-tenant clinic POS, 1 Tenant มีหลาย Branch, 1 Patient อยู่ใน Tenant เดียว
> วิธีไหนปลอดภัยที่สุดที่จะป้องกันไม่ให้ developer ดึงข้อมูลข้าม Tenant โดยไม่ตั้งใจ?"

**Claude เสนอ 3 ทาง:**
- Middleware filter
- EF Core Global Query Filters
- PostgreSQL Row-Level Security

**ที่เลือกและเหตุผล:**
เลือก EF Core Global Query Filters เพราะ filter อยู่ใน DbContext เลย ทุก query ได้รับ filter อัตโนมัติ ไม่ต้องพึ่งให้ developer จำทุกครั้ง ต่างจาก Middleware ที่ถ้าลืมก็รั่วทันที ส่วน RLS ดีแต่ซับซ้อนเกินสำหรับ v1

**ที่ reject:**
Claude แนะนำให้ใส่ Middleware เพิ่มเป็น "defense in depth" ด้วย บอกไปว่าไม่เอา ซ้ำซ้อนและเพิ่ม complexity โดยไม่จำเป็น

---

## Prompt 2 — TenantId มาจากไหน

**ที่ถาม:**
> "ให้ TenantId มาจาก JWT claims เท่านั้น ห้ามมาจาก request body เด็ดขาด"

**Claude ผิดอะไร:**
ครั้งแรก Claude ใส่ TenantId ใน static field — reject ทันที เพราะ static field ไม่ thread-safe ใน async context

**ที่ได้:**
ใช้ `ICurrentTenantService` inject เข้า DbContext constructor แล้วอ่านจาก `HttpContext.User` claims แทน

**iteration เพิ่ม:**
ถามต่อว่า "แล้ว Seeder ล่ะ? ต้องเขียนข้อมูลข้าม Tenant ได้" Claude แนะนำให้ทำ `SeederDbContext` แยก ที่ bypass filter ได้ ใช้ได้ดี

---

## Prompt 3 — เบอร์โทรซ้ำ

**ที่ถาม:**
> "PhoneNumber ต้อง unique ภายใน Tenant เดียวกัน ถ้า 2 request ส่งมาพร้อมกันต้องไม่ผ่านทั้งคู่"

**Claude แนะนำ:**
เช็คใน application layer ก่อน แล้ว rely on DB unique index เป็น safety net

**ที่คิดเพิ่ม:**
การเช็คใน application layer อย่างเดียวมี race condition (TOCTOU) อยู่ DB unique index บน `(TenantId, PhoneNumber)` ต่างหากที่ป้องกันจริงๆ ส่วน application check ไว้แค่ให้ error message ดูดีขึ้น จาก 500 เป็น 409

---

## Prompt 4 — นัดซ้ำ

**ที่ถาม:**
> "ป้องกัน appointment ซ้ำ: patient เดิม + branch เดิม + เวลาเดิม ภายใน tenant เดียวกัน ต้อง safe ภายใต้ concurrency"

**Claude เสนอครั้งแรก:**
Pessimistic lock (`SELECT FOR UPDATE`)

**ที่เลือก:**
DB unique index บน `(TenantId, PatientId, BranchId, StartAt)` แทน ง่ายกว่า PostgreSQL จัดการ race condition เองได้เลย ไม่ต้องมาจัดการ lock เอง

---

## Prompt 5 — RabbitMQ event ส่งตอนไหน

**ที่ถาม:**
> "Event AppointmentCreated ควร publish ใน transaction หรือหลัง commit?"

**Claude อธิบาย trade-off:**
In-transaction + outbox = guaranteed แต่ซับซ้อน, After-commit = ง่ายแต่ถ้า process crash ระหว่าง save กับ publish event หาย

**ที่ตัดสินใจ:**
After-commit สำหรับ v1 บันทึก trade-off ไว้ใน README แล้ว ไม่คุ้มที่จะทำ outbox pattern ใน 90 นาที

---

## Prompt 6 — Authorization

**ที่ถาม:**
> "Viewer ดูได้แต่สร้าง patient ไม่ได้ Admin กับ User สร้างได้ วิธีที่ง่ายที่สุด?"

**Claude เสนอครั้งแรก:**
Custom `IAuthorizationRequirement` policy

**ที่ใช้จริง:**
`[Authorize(Roles = "Admin,User")]` บน POST, `[Authorize(Roles = "Admin,User,Viewer")]` บน GET แค่นี้พอ 3 roles ไม่จำเป็นต้องมี policy engine

---
