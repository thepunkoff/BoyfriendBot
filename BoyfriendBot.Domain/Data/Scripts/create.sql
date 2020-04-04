CREATE TABLE "User" ("ChatId" long primary key, "Username" text, "UserId" long, "FirstName" text, "LastName" text);


CREATE TABLE "RarityWeights"
(
	"ChatId" long,
	"WhiteWeight" int not null default 100,
	"GreenWeight" int not null default 20,
	"BlueWeight" int not null default 10,
	"PurpleWeight" int not null default 5,
	"OrangeWeight" int not null default 1,
    FOREIGN KEY ("ChatId") REFERENCES "User"("ChatId")

);


CREATE TABLE "UserSettings"
(
	"ChatId" long,
	"RecieveReminders" boolean not null default true,
	"RecieveScheduled" boolean not null default true, "Gender" boolean not null default 0, "BotGender" boolean not null default 1,
    FOREIGN KEY ("ChatId") REFERENCES "User"("ChatId")
);