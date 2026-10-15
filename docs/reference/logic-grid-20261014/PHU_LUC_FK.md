# FK người xử lý trình bày riêng

18 quan hệ dưới đây cùng trỏ Account(account_id). 32 quan hệ chính được thể hiện trong hình, không thay đổi schema.

| Bảng con | Cột FK | Bảng cha |
| --- | --- | --- |
| Registration | admission_approved_by | Account |
| Registration | cancelled_by | Account |
| Registration | rejected_by | Account |
| EntryTestSession | cancelled_by | Account |
| EntryTestAttempt | graded_by | Account |
| EntryTestBooking | absent_by | Account |
| EntryTestBooking | cancelled_by | Account |
| Class | cancelled_by | Account |
| StudentClassAssignment | assigned_by | Account |
| Schedule | held_by | Account |
| Schedule | not_held_by | Account |
| Attendance | entered_by | Account |
| Score | entered_by | Account |
| GradeReport | reviewed_by | Account |
| GradeReport | submitted_by | Account |
| Invoice | cancelled_by | Account |
| Payment | confirmed_by | Account |
| Payment | last_corrected_by | Account |

Danh mục đầy đủ trong DANH_MUC_50_FK.csv.
