CREATE TABLE `AdminValues` (
  `id` int(11) NOT NULL auto_increment,
  `tec` varchar(20) NOT NULL,
  `gtp` varchar(20) NOT NULL,
  `date` datetime NOT NULL,
  `recommendation` double NOT NULL default '0',
  `is_percent` binary(1) NOT NULL default '0',
  `diviation` double default '0',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=70 DEFAULT CHARSET=cp1251 AUTO_INCREMENT=70 ;

-- 
-- Комментарии к таблице `AdminValues`:
--   `id`
--       `идентификатор`
--   `date`
--       `дата для применения`
--   `recommendation`
--       `дополнение для планового значения`
--   `is_percent`
--       `отклонение указано в процентах`
--   `diviation`
--       `отклонение`
--   `tec`
--       `имя тэц`
--   `gtp`
--       `имя гтп (если есть)`
-- 