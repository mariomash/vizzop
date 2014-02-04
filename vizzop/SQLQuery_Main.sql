/*
* Tamaños de SQL
*/
--Tamaño completo
SELECT SUM(reserved_page_count) * 8.0 / 1024 AS 'Table Size (MB)'
	FROM sys.dm_db_partition_stats
--Tamaño ordenado por tabla
 SELECT sys.objects.name,
	SUM(row_count) AS 'Row Count',
	SUM(reserved_page_count) * 8.0 / 1024 AS 'Table Size (MB)'
	FROM sys.dm_db_partition_stats, sys.objects
	WHERE sys.dm_db_partition_stats.object_id = sys.objects.object_id
	GROUP BY sys.objects.name
	ORDER BY [Table Size (MB)] DESC