# M3 — ba đợt giảng dạy và công bố kết quả

Ngày chia: 2026-10-12. Owner đã giao đợt1/gói1–3 vào2026-10-12; đợt1 COMPLETE/review PASS/applied ADR163/main reverified (source04ba2a5), owner continuation đã giao đợt2/gói4–5, nay COMPLETE/PASS/applied/main reverified (source d289f60); đợt3 vẫn PLANNED/chưa giao.
Baseline main e239951 / source050d5d9: M0/M1/M2 COMPLETE, M2 review PASS/applied ADR163/main reverified. Các header M1 cũ trong Memory/handoff là provenance; checkpoint hiện tại ở plan.md và M2-schedule-evidence/summary.json.

## Phân chia 8 gói

| Đợt | Gói | Phạm vi và đầu ra |
| --- | --- | --- |
| 1 — Vận hành giảng dạy | 1. Bắt đầu lớp và xác nhận buổi đã tổ chức | API100,125–126; mở START trong preview098, xác nhận OPEN→ONGOING và HELD riêng từng occurrence. UI Giáo vụ, không tự suy HELD từ thời gian/điểm danh. |
| 1 | 2. Điểm danh từng buổi | API131–135; Teacher xem/lưu delta một LESSON all-or-none, Giáo vụ chỉ đọc; Chưa ghi khác Có mặt/Vắng. |
| 1 | 3. Hoàn thành lớp | API101 và COMPLETE trong preview098; kiểm toàn bộ nghĩa vụ hiện hành, HELD và coverage điểm danh, chuyển ONGOING→COMPLETED rồi khóa điểm danh. |
| 2 — Điểm nháp và vòng xét duyệt | 4. Sổ điểm và lưu một học viên | API136–139; frozen components, schedule options, scores/comment một học viên cùng transaction; tổng tính ở server bằng FN05, missing khác0. |
| 2 | 5. Gửi, trả lại, sửa và gửi lại cả lớp | API140–144,146 và action RETURN của145; Teacher submit/resubmit, Giáo vụ review qua SP03 và reasoned return. Hoàn chỉnh vòng DRAFT→SUBMITTED→RETURNED→SUBMITTED; mọi thao tác cả cohort, không chốt/công bố trong đợt này. |
| 3 — Công bố và hoàn tất M3 | 6. Chốt toàn lớp | API147 và action FINALIZE của145; final_score/rounded total, trạng thái toàn cohort, Registration.is_unfinished theo FN03, audit/T23 cùng transaction. FINALIZED bất biến. |
| 3 | 7. Học viên xem kết quả công bố | API148–149; danh sách/chi tiết own FINALIZED, component/weight/score/comment/final_score/pass; không lộ điểm nháp hoặc người khác. |
| 3 | 8. Nghiệm thu tích hợp M3 | Luồng từ lớp/schedule/placement M2 tới teaching/attendance/draft/return/resubmit/finalize/Student publication; regression M0–M2, review PASS/apply/reverify main và genuine LOCAL_DEV. Gói này không thêm API nghiệp vụ. |

Thứ tự phụ thuộc: gói1→2→3, gói4→5, gói6→7→8; đợt2 dùng lớp đã COMPLETED từ đợt1. Điểm nháp được phép ở ONGOING/COMPLETED theo contract; submit/resubmit chỉ sau COMPLETED. RETURN ở đợt2 để kiểm thử vòng sửa/gửi lại thật, tránh tách một workflow chưa dùng được. M3 bổ sung23 API IDs (100–101,125–126,131–149), trong đó9 commands; tái sử dụng098 preview,090 caller lookup và các Class/calendar đã làm ở M2.

## Tiêu chí kết thúc từng đợt

- Đợt1: START cần đầy đủ LESSON theo Program và ít nhất một placement; không tự yêu cầu exam trước START. HELD từ scheduled end, Class ONGOING/current PLANNED, không đòi điểm danh đủ. Điểm danh từ lesson start tới trước COMPLETED, không thêm prerequisite ONGOING hoặc hạn24/48h. COMPLETE cần đủ LESSON và một FINAL_EXAM hiện hành HELD, đầy đủ điểm danh LESSON toàn cohort; Vắng là recorded hợp lệ, FINAL_EXAM không có điểm danh mới. Chưa kết thúc Registration hoặc gửi/chốt điểm.
- Đợt2: thiếu điểm/blank khác0, omitted giữ nguyên, không xóa Score; GENERAL không schedule, LESSON/FINAL_EXAM đúng current Class/type và từ start. Score-only save touch GradeReport RV; scores/comment rollback cùng nhau. DRAFT/RETURNED editable, SUBMITTED khóa; Attendance vẫn khóa khi Class COMPLETED. Unsaved edits phải được xử lý trước whole-Class preview. Submit/return/resubmit toàn bộ cohort hoặc không ai; return có lý do/giữ scores-comment, Giáo vụ không đọc DRAFT values, Student chưa đọc grades.
- Đợt3: finalize full live cohort, decimal/half-up một lần/max2 decimals input, thiếu≠0, ngưỡng đạt50 sau rounding; lưu final_score và giải phóng unfinished enrollment atomic. Không reopen/override FINALIZED. Student chỉ own FINALIZED, current assignment/session/privacy đúng đến cuối response. Luồng nhiều khóa/học lại không lẫn kết quả.

Mỗi đợt sau khi được giao phải có task card/contract/phạm vi chia sẻ và allowlist cụ thể trước delegation. Codex là manager; antigravity-worker chỉ pure bounded helper/form với context tối thiểu đã định, không tự gửi toàn source/report/owner/secrets. Codex kiểm evidence/scope, tự tích hợp và review; ADR177/163 giữ nguyên, không hỏi lại các quyết định đã chốt.

Mỗi đợt có backend/API/UI hoàn chỉnh theo phạm vi, default/build/frontend/types/lint, real owned scratch SQL/HTTP, audit rollback/absence/membership/RV/races/cutoff/replay/current-role tests, Edge375/768/1280 và mất postcommit ACK/manual lookup/no resend. Review PASS mới apply đúng snapshot main; reverify main/LOCAL_DEV và ID/GUID cleanup/owner hashes. Gói8 đóng toàn M3, không dùng PASS từng đợt để tuyên bố M3 hoàn tất sớm.

Các races bắt buộc: Attendance vs COMPLETE; Score save vs submit; HELD/complete vs replacement/move; current Teacher reassignment/block/session expiry; absent-child concurrent insert; whole-cohort changes vs submit/return/finalize; hai intent finalize và duplicate UUID. Kiểm FN05/SP03/FN03 thực tế, trigger TG04/TG05 và TX10–12, không thay bằng success mocks hoặc suy quyền từ nút UI.

## Phạm vi và nguồn

Nguồn canonical: requirements.md (ADR024/025/028/033/034/087/124/125), E04_CONTRACT_CATALOG (API IDs), E04_DTO_SPEC §§16,22,24–29, E04_FINAL_CONTRACT, E04_SQL_INTEGRATION_SPEC, E04_REPLAY_SPEC và implementation-plan milestoneM3. Exact body/role/membership/audit/lock protocol theo các nguồn này, không dùng phần draft lịch sử trong SYSTEM_ARCHITECTURE_SPEC để tự thêm quyết định.

Tái sử dụng24-table schema/6migrations/grants/EC-C1/T23 và existing SQL objects; không mặc định cần migration/package mới. M4 reports/CSV/audit viewer nằm ngoài M3; không thêm auto HELD/auto submit, import Excel, autosave/durable drafts, Student attendance API, score deletion, chuyển lớp, mở chốt, runtime provisioning, install/push/deploy. Owner4 untracked giữ nguyên, không đọc/share/stage; D07 không chạy lại. LOCAL_DEV/preview10000/custody decisions giữ nguyên.

Trạng thái hiện tại: đợt1 COMPLETE/PASS/applied/main reverified, whole M3 IN PROGRESS. M3_01_TASK_CARD/REVIEW và M3-teaching-evidence/summary.json giữ proof1391default/96uniqueSQLHTTP/136frontend/7Edge+final92/genuine LOCAL_DEV PASS. Bước tiếp theo khi được giao đợt2: chuẩn bị exact contract/sharing scope cho gói4–5; không tự mở finalize/M4 hoặc dispatch worker chỉ để lập kế hoạch.

Closure2026-10-12: đợt2 COMPLETE/Codex independent review PASS/applied ADR163/main reverified. Source main d289f60/manager bf7a6a8;1391default/272SKIP/117SQLHTTP/144frontend/8Edge+final93/genuine LOCAL_DEV PASS. M3_02_TASK_CARD/REVIEW/summary giữ proof. Đợt3/gói6–8 vẫn planned/unassigned;147/Student publication/unfinished clear chưa implement. Historical assignment descriptions above retain their original checkpoint.
