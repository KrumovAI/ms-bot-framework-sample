namespace BasicBot.Infrastructure.Constants
{
    public class MessageConstants
    {
        // General
        public const string MessageNotUnderstood = "Ко каза ко?";

        public const string WhatCanIDoForYou = "Какво мога да направя за Вас!";

        public const string UnauthorizedRequest = "Нямате достъп до дадения ресурс!";

        public const string BadRequest = "Възникна грешка при заявката!";

        public const string InvalidChoice = "Моля изберете някоя от предоставените опции!";

        // Auth
        public const string Login = "Вход";

        public const string PleaseLogin = "Моля влезте в акаунта си!";

        public const string PleaseInsertAuthCode = "Поставете получения код тук!";

        public const string InvalidAuthCode = "Невалиден код! Моля, опитайте отново!";

        public const string SuccessfulLogin = "Входът бе успешен!";

        public const string UnsuccessfulLogin = "Входът неуспешен! Опитайте пак!";

        public const string SuccessfulLogout = "Излязохте успешно от акаунта си!";

        // Custom
        public const string TeacherNotParsed = "Не разбрах името на учителя!";

        public const string TeacherNotFound = "Учителят не е намерен!";

        public const string TooManyTeachersFound = "Бяха намерени повече от един учител, моля, изберете вашия избор!";

        public const string TeacherHasNoClassMessage = "Учителят е свободен вмомента!";

        public const string TeacherHasNoMoreClassesForToday = "Учителят е свободен до края на деня!";
    }
}
