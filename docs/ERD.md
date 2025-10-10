# ER Diyagramı — KutuphaneOtomasyonu

Aşağıdaki ER diyagramı, uygulamadaki temel varlıkları (Books, Copies, Members, Loans), birincil anahtarları, benzersiz kısıtları ve ilişkileri göstermektedir.

```mermaid
erDiagram
    BOOKS {
        int BookId PK
        string Isbn "UNIQUE"
        string Title
        string Author
        int PublishYear
        string Category
    }

    COPIES {
        int CopyId PK
        int BookId FK
        string ShelfLocation
        string Status "ENUM: available|loaned|damaged|lost"
    }

    MEMBERS {
        int MemberId PK
        string FullName
        string Email "UNIQUE"
        string Phone
        datetime JoinedAt
        string Status "ENUM: active|passive"
    }

    LOANS {
        int LoanId PK
        int CopyId FK
        int MemberId FK
        datetime LoanedAt
        datetime DueAt
        datetime ReturnedAt "NULLABLE"
    }

    BOOKS ||--o{ COPIES : "has many"
    COPIES ||--o{ LOANS : "has many"
    MEMBERS ||--o{ LOANS : "has many"
```

## İş Kuralları ve Kısıtlar (Özet)
- Sadece `Status = available` olan kopyalar ödünç verilebilir.
- Bir üyenin aynı anda en fazla 3 aktif (ReturnedAt IS NULL) ödünç kaydı olabilir.
- Ödünçte `DueAt = LoanedAt + 14 gün`.
- Ödünç verildiğinde `Copy.Status = loaned`, iade edildiğinde `Copy.Status = available`.

## İndeksler ve Benzersizlikler (SQL Server)
- `Books(Isbn)` üzerinde UNIQUE indeks
- `Members(Email)` üzerinde UNIQUE indeks
- Yabancı anahtarlar:
  - `Copies(BookId) → Books(BookId)` (ON DELETE RESTRICT)
  - `Loans(CopyId) → Copies(CopyId)` (ON DELETE RESTRICT)
  - `Loans(MemberId) → Members(MemberId)` (ON DELETE RESTRICT)

## Notlar
- `Status` alanları veritabanında okunabilirlik için `ENUM` karşılığı olarak `string` saklanır.
- Tarihler UTC olarak tutulur (`LoanedAt`, `DueAt`, `ReturnedAt`).

