-- Script Date: 05-May-20 22:54  - ErikEJ.SqlCeScripting version 3.5.2.86
CREATE TABLE [flights] (
  [flight_id] INTEGER NOT NULL
, [company] TEXT NOT NULL
, [flight_name] TEXT NOT NULL
, [passesngers] INTEGER DEFAULT (0) NOT NULL
, [init_longitude] NUMERIC DEFAULT (0) NOT NULL
, [init_latitude] NUMERIC DEFAULT (0) NOT NULL
, [takeoff_time] NUMERIC NOT NULL
, [takeoff_time_unix] NUMERIC DEFAULT (0) NOT NULL
, CONSTRAINT [PK_flights] PRIMARY KEY ([flight_id])
);
