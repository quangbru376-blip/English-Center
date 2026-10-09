SELECT
 IS_ROLEMEMBER(N'ec_app_runtime') AS is_runtime,
 CASE WHEN USER_NAME()=N'dbo' THEN 1 WHEN USER_NAME() IS NOT NULL THEN 0 END AS is_dbo,
 IS_SRVROLEMEMBER(N'sysadmin') AS is_sysadmin,
 HAS_PERMS_BY_NAME(DB_NAME(),N'DATABASE',N'CONTROL') AS controls_database,
 CASE WHEN HAS_PERMS_BY_NAME(NULL,NULL,N'CONTROL SERVER') IS NULL
  OR NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE type=N'R' AND name=N'public') THEN NULL
 WHEN HAS_PERMS_BY_NAME(NULL,NULL,N'CONTROL SERVER')=1
  OR EXISTS (SELECT 1 FROM sys.server_principals AS p JOIN sys.login_token AS t
   ON t.principal_id=p.principal_id WHERE p.type=N'R' AND p.name<>N'public')
  OR EXISTS (SELECT 1 FROM sys.server_permissions AS p JOIN sys.login_token AS t
   ON t.principal_id=p.grantee_principal_id
   WHERE t.principal_id<>(SELECT principal_id FROM sys.server_principals WHERE type=N'R' AND name=N'public')
    AND NOT (p.class=100 AND p.permission_name=N'CONNECT SQL' AND p.state=N'G'))
 THEN 1 ELSE 0 END AS controls_server,
 CASE WHEN EXISTS (
  SELECT 1 FROM sys.database_principals
  WHERE type=N'R' AND name NOT IN (N'public',N'ec_app_runtime')
   AND (IS_ROLEMEMBER(name)=1 OR IS_ROLEMEMBER(name) IS NULL)
 ) THEN 1 ELSE 0 END AS extra_database_role,
 CASE WHEN EXISTS (
  SELECT 1 FROM sys.database_permissions AS p
  JOIN sys.user_token AS t ON t.principal_id=p.grantee_principal_id
  WHERE t.principal_id NOT IN (DATABASE_PRINCIPAL_ID(N'public'),DATABASE_PRINCIPAL_ID(N'ec_app_runtime'))
   AND NOT (
    p.state=N'G' AND (
     (p.class=0 AND p.permission_name IN (N'CONNECT',N'VIEW DEFINITION'))
     OR (p.class=1 AND p.permission_name=N'SELECT'
      AND p.major_id=ISNULL(OBJECT_ID(N'dbo.__EFMigrationsHistory'),-1)
      AND p.minor_id IN (
       ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.__EFMigrationsHistory'),N'MigrationId',N'ColumnId'),-1),
       ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.__EFMigrationsHistory'),N'ProductVersion',N'ColumnId'),-1)
      )
     )
    )
   )
 ) THEN 1 ELSE 0 END AS direct_effective_grant,
 CONVERT(int,SERVERPROPERTY(N'EngineEdition')) AS engine_edition;
