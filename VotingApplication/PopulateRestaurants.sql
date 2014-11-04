USE VotingApplication
DELETE FROM dbo.Options;
INSERT INTO dbo.Options (Name, Description, Info) VALUES
	('Guissepes', 'Italian on the steps', '0117 926 4869'),
	('Bella Vista', 'Italian next door', '0117 925 0929'),
	('Sticks & Broth', 'Ramen and Donburi', '0117 925 5397'),
	('4500 miles from Dheli', 'Indian Buffet', '0117 929 2224'),
	('Turtle Bay', 'Carribbean', '0117 929 0209'),
	('Marcos Olive Branch', 'Italian the other way', '0117 922 6688'),
	('Mezze Palace', 'Lebanese', '0117 927 7937'),
	('Europa', 'Slug & Lettuce', '0117 929 7818'),
	('Dynasty', 'Chinese crispy duck pancakes', '0117 925 0888'),
	('Small Bar', 'Hotdogs and Sandwiches', 'N/A'),
	('Start the Bus', 'Burgers', '0117 930 4370');
DELETE FROM dbo.Sessions;
INSERT INTO dbo.Sessions (Name) VALUES ('Main Session'), ('Other Session');
