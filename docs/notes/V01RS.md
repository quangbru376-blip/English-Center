# V01-R/S
1. R creates a WITHOUT LOGIN fixture only inside one rollback transaction per case.
2. Its runtime-role membership proves catalog visibility, not real login authority.
3. Database VIEW DEFINITION plus two history-column SELECT grants read880/880 facts.
4. Ablation proves explicit VIEW SECURITY DEFINITION was redundant here.
5. S declares these exact read permissions in the principal guard allowlist.
6. State W/D, broad table SELECT and direct business permissions still deny.
7. Unknown metadata IDs cannot match a grant exception.
8. Existing non-dbo/runtime/no sysadmin/CONTROL/extra-role checks remain unchanged.
9. Three SQL grant cases PASS, then all fixture users/grants/memberships rollback.
10.D07 catalog/profile/exact102rolepermissions hash remains identical.
11.Three actual diagnostic denial tests still PASS on manager/main.
12.No permanent provisioning or positive runtime readiness is claimed.
