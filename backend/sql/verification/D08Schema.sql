IF DB_NAME() <> N'EnglishCenter_D07_20261005' THROW 51900,'Wrong isolated database',1;
IF (SELECT COUNT(*) FROM sys.triggers WHERE parent_class=1 AND is_ms_shipped=0)<>5
 OR EXISTS(SELECT 1 FROM sys.triggers WHERE is_disabled=1 AND is_ms_shipped=0)
 OR (SELECT COUNT(*) FROM sys.trigger_events e JOIN sys.triggers t ON t.object_id=e.object_id WHERE t.is_ms_shipped=0)<>14
 THROW 51900,'Trigger count/events/enabled mismatch',1;
IF EXISTS(SELECT name,tableName FROM (VALUES
 ('trg_Program_UsedConfigurationGuard','TrainingProgram'),
 ('trg_Component_UsedConfigurationGuard','ScoreComponentConfig'),
 ('trg_Band_ThresholdSetFreezeGuard','Band'),
 ('trg_Attendance_ClassScheduleGuard','Attendance'),
 ('trg_Score_ComponentScheduleGradeGuard','Score')) expected(name,tableName)
 EXCEPT SELECT t.name,OBJECT_NAME(t.parent_id) FROM sys.triggers t WHERE t.is_ms_shipped=0)
 THROW 51900,'Trigger table/name mismatch',1;
IF EXISTS(SELECT 1 FROM dbo.Course) OR EXISTS(SELECT 1 FROM dbo.Account)
 OR EXISTS(SELECT 1 FROM dbo.Role) OR EXISTS(SELECT 1 FROM dbo.Band)
 THROW 51900,'Fixture data was not rolled back',1;
IF (SELECT COUNT(*) FROM sys.tables WHERE name<>'__EFMigrationsHistory')<>24
 OR (SELECT COUNT(*) FROM sys.foreign_keys)<>50
 OR (SELECT COUNT(*) FROM sys.check_constraints)<>70
 THROW 51900,'Physical schema changed',1;
PRINT 'D08 five enabled triggers/events/tables and fixture rollback PASS';
