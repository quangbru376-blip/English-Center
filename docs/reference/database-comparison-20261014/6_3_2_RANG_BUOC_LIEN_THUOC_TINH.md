# 6.3.2 — Đối chiếu ràng buộc liên thuộc tính trong một bộ

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Đối chiếu tám nhóm trong báo cáo

| Bảng | Báo cáo 6.3.2 | Hiện tại | Cách sửa |
| --- | --- | --- | --- |
| Account | locked_until chỉ khi LOCKED, timer khóa tạm | Không có locked_until. LoginThrottledUntil là timer login độc lập với ACTIVE/LOCKED/DISABLED | Sai cột/mô hình; thay bằng failed-login window/throttled-until và temp bundle. |
| EntryTestSession | end_at>start_at | Khớp; CK_Time còn kiểm phút và cùng ngày Việt Nam | Giữ và bổ sung chính xác cửa sổ/lý do khi hủy. |
| Registration | ApprovedBundle chỉ ADMITTED; confirmation trước/ sau; không đề cập giữ sau hủy | CK ApprovedBundle chỉ buộc by/at cùngNULL/cùngcó; workflow giữ confirmation/approval khi CANCELLED sau ADMITTED. Rejected/Cancelled có full bundle/prior-state/operation | Không xóa lịch sử theo status hiện tại; tách CK với state machine service. |
| EntryTestAttempt | status COMPLETED buộc score/by/at, trạng thái khác NULL | Không status. CK_ResultBundle: score/by/at/completed_booking_id cùngNULL hoặc cùngcó | Thay bằng bốn trường; trạng thái COMPLETED kiểm trên Booking liên quan. |
| Schedule | end_time>start_time | Khớp nhưng thiếu seconds=0, current/state, held/not-held bundles và replacement metadata | Bổ sung đầy đủ; thay thế không xóa source occurrence. |
| GradeReport | Submit/review required theo state; FINALIZED reason phảiNULL | Submit/review bundle gồm operation IDs. FINALIZED cần score/finalized/review, finalizedAt=reviewedAt; ReturnedReason chỉ bắt buộc khi RETURNED, không CHECK épNULL ở FINALIZED | Sửa rule reason; metadata gần nhất có thể giữ qua resubmit/finalize. |
| Invoice | due_date không trước ngày issue | Service issue lấy ngày VN+7 lịch, không chỉ≥issue; CK invoice còn number/amount/cancel bundle | Nêu +7 lịch và snapshot, không suy CK đơn bảng đảm bảo workflow. |
| Payment | BANK_TRANSFER cần reference | TRANSFER cần reference; correction by/at/reason cùngNULL hoặc cùngcó; amount>0 | Sửa enum, thêm correction bundle và bất biến paid/link/amount. |

## 2. Các bundle/cặp thuộc tính bị bỏ sót

- Account.must_change_password=true: temp_issued_at/temp_expires_at cùng có và expires=issued+24h; false: hai mốc NULL.
- AuthSession: issued≤last_activity≤expires, expires>issued, revoke NULL hoặc≥issued; loại scope và role authority độc lập.
- EntryTestBooking: ABSENT có absent_by/at/operation; CANCELLED có cancel actor/time/prior SCHEDULED/operation; giữ các bundle theo đúng source state.
- Class/EntryTestSession/Invoice: cancellation bundles đầy đủ với reason/operation/prior-state theo từng CHECK.
- Schedule: current=1 cho PLANNED/HELD, current=0 cho NOT_HELD; HeldBundle và NotHeldBundle tách nhau; no self replacement.
- AuditLog: SECURITY có security expiry≥occurred+90d, MANDATORY expiryNULL; IP có thì IP expiry đúng+90d; details JSON khi có.
- ReplayOperation: JSON result reference, mode đúng và expiry≥completed+24h.

## 3. Predicate CHECK cụ thể để cập nhật

| Bảng | CHECK | Predicate đang có |
| --- | --- | --- |
| Account | CK_Account_TempBundle | ([must_change_password]=(1) AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at]=dateadd(hour,(24),[temp_issued_at]) OR [must_change_password]=(0) AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL) |
| AuthSession | CK_AuthSession_Lifetime | ([issued_at]<=[last_activity_at] AND [last_activity_at]<=[expires_at] AND [expires_at]>[issued_at]) |
| AuthSession | CK_AuthSession_RevokedAt | ([revoked_at] IS NULL OR [revoked_at]>=[issued_at]) |
| Registration | CK_Registration_ApprovedBundle | ([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL OR [admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL) |
| Registration | CK_Registration_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND (([cancelled_from_status]) collate Latin1_General_100_BIN2='ADMITTED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='CONFIRMED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='BAND_ASSIGNED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='TESTING' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='PENDING') AND datalength([cancelled_from_status])=len([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL) |
| Registration | CK_Registration_RejectedBundle | ([status]='REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND len([rejection_reason])>(0) AND [rejected_from_status] IS NOT NULL AND ([rejected_from_status]) collate Latin1_General_100_BIN2='CONFIRMED' AND datalength([rejected_from_status])=len([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL OR [status]<>'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL) |
| EntryTestSession | CK_EntryTestSession_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND len([cancellation_reason])>(0) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| EntryTestSession | CK_EntryTestSession_Time | ([start_at]<[end_at] AND datepart(second,[start_at])=(0) AND datepart(millisecond,[start_at])=(0) AND datepart(second,[end_at])=(0) AND datepart(millisecond,[end_at])=(0) AND CONVERT([date],dateadd(minute,(420),[start_at]))=CONVERT([date],dateadd(minute,(420),[end_at]))) |
| EntryTestAttempt | CK_EntryTestAttempt_ResultBundle | ([score] IS NULL AND [graded_by] IS NULL AND [graded_at] IS NULL AND [completed_booking_id] IS NULL OR [score] IS NOT NULL AND [graded_by] IS NOT NULL AND [graded_at] IS NOT NULL AND [completed_booking_id] IS NOT NULL) |
| EntryTestBooking | CK_EntryTestBooking_AbsenceBundle | ([status]='ABSENT' AND [absent_by] IS NOT NULL AND [absent_at] IS NOT NULL AND [absent_operation_id] IS NOT NULL OR [status]<>'ABSENT' AND [absent_by] IS NULL AND [absent_at] IS NULL AND [absent_operation_id] IS NULL) |
| EntryTestBooking | CK_EntryTestBooking_CancellationBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND ([cancelled_from_status]) collate Latin1_General_100_BIN2='SCHEDULED' AND datalength([cancelled_from_status])=len([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| Class | CK_Class_CancellationBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND len([cancellation_reason])>(0) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| Schedule | CK_Schedule_CurrentState | (([occurrence_state]='HELD' OR [occurrence_state]='PLANNED') AND [is_current]=(1) OR [occurrence_state]='NOT_HELD' AND [is_current]=(0)) |
| Schedule | CK_Schedule_HeldBundle | ([occurrence_state]='HELD' AND [held_by] IS NOT NULL AND [held_at] IS NOT NULL OR [occurrence_state]<>'HELD' AND [held_by] IS NULL AND [held_at] IS NULL) |
| Schedule | CK_Schedule_NoSelfReplacement | ([replaces_schedule_id] IS NULL OR [replaces_schedule_id]<>[schedule_id]) |
| Schedule | CK_Schedule_NotHeldBundle | ([occurrence_state]='NOT_HELD' AND [not_held_by] IS NOT NULL AND [not_held_at] IS NOT NULL AND [not_held_reason] IS NOT NULL AND len([not_held_reason])>(0) AND [replacement_operation_id] IS NOT NULL OR [occurrence_state]<>'NOT_HELD' AND [not_held_by] IS NULL AND [not_held_at] IS NULL AND [not_held_reason] IS NULL AND [replacement_operation_id] IS NULL) |
| Schedule | CK_Schedule_Time | ([start_time]<[end_time] AND datepart(second,[start_time])=(0) AND datepart(second,[end_time])=(0)) |
| GradeReport | CK_GradeReport_FinalResult | ([status]='FINALIZED' AND [final_score] IS NOT NULL AND [final_score]>=(0) AND [final_score]<=(100) AND [finalized_at] IS NOT NULL AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL AND [finalized_at]=[reviewed_at] OR [status]<>'FINALIZED' AND [final_score] IS NULL AND [finalized_at] IS NULL) |
| GradeReport | CK_GradeReport_ReturnedReason | ([status]<>'RETURNED' OR [returned_reason] IS NOT NULL AND len([returned_reason])>(0) AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL) |
| GradeReport | CK_GradeReport_ReviewBundle | ([reviewed_by] IS NULL AND [reviewed_at] IS NULL AND [review_operation_id] IS NULL OR [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL) |
| GradeReport | CK_GradeReport_SubmitBundle | ([submitted_by] IS NOT NULL AND [submitted_at] IS NOT NULL AND [submitted_operation_id] IS NOT NULL OR [status]='DRAFT' AND [submitted_by] IS NULL AND [submitted_at] IS NULL AND [submitted_operation_id] IS NULL) |
| Invoice | CK_Invoice_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancellation_operation_id] IS NOT NULL AND ([cancelled_from_status]) collate Latin1_General_100_BIN2='UNPAID' AND datalength([cancelled_from_status])=len([cancelled_from_status]) OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL) |
| Payment | CK_Payment_CorrectionBundle | ([last_corrected_by] IS NULL AND [last_corrected_at] IS NULL AND [last_correction_reason] IS NULL OR [last_corrected_by] IS NOT NULL AND [last_corrected_at] IS NOT NULL AND [last_correction_reason] IS NOT NULL AND len([last_correction_reason])>(0)) |
| AuditLog | CK_AuditLog_IPBundle | ([ip_address] IS NULL AND [ip_expires_at] IS NULL OR [ip_address] IS NOT NULL AND [ip_expires_at] IS NOT NULL AND [ip_expires_at]=dateadd(day,(90),[occurred_at])) |
| AuditLog | CK_AuditLog_SecurityExpiry | ([audit_category]='SECURITY' AND [security_expires_at] IS NOT NULL AND [security_expires_at]>=dateadd(day,(90),[occurred_at]) OR [audit_category]='MANDATORY' AND [security_expires_at] IS NULL) |
| ReplayOperation | CK_ReplayOperation_Expiry | ([expires_at]>=dateadd(hour,(24),[completed_at])) |

SQL CHECK chứa NULL phải dùng IS NULL/IS NOT NULL rõ; SQL UNKNOWN không thay guard. Thời điểm server, role của actor và trạng thái parent là điều kiện workflow/service, không chỉ bundle CHECK. Không nhận actor/time/client tự khai để bỏ kiểm phiên.

## 4. Ví dụ sửa thuyết minh

Thay đoạn “EntryTestAttempt.status=COMPLETED…” bằng: EntryTestAttempt lưu kết quả đầu vào gồm score, graded_by, graded_at và completed_booking_id; bốn trường cùng NULL khi chưa có kết quả hoặc cùng có khi đã chấm. Foreign key ghép bảo đảm completed Booking thuộc đúng Attempt; service kiểm booking COMPLETED và quyền grader. ABSENT không tạo score0.

Thay đoạn “FINALIZED phải xóa returned_reason” bằng: RETURNED bắt buộc có lý do; khi gửi lại/chốt, dữ liệu review/submit gần nhất và latest return reason được giữ theo implementation, audit giữ lịch sử các lần. FINALIZED yêu cầu final_score/finalized_at và review bundle đầy đủ, không mở lại.


Nguồn dùng chung và phương pháp: [README](README.md).
