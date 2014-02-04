namespace vizzopWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using vizzopWeb.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<vizzopContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(vizzopContext context)
        {
            bool texttrings = false;
            bool isocodes = false;
            bool countries = false;
            bool servicetypes = false;

            try
            {
                if (texttrings == true)
                {
                    context.TextStrings.AddOrUpdate(
                        new TextString { Ref = "law_disclaimer", IsoCode = "es", Text = "Esta web usa sistemas de monitorización. Navegando en ella usted acepta nuestra política de monitorización" },
                        new TextString { Ref = "law_disclaimer", IsoCode = "en", Text = "This site uses monitoring systems. By browsing the site, you're agreeing to our monitoring policy." },
                        new TextString { Ref = "loading", IsoCode = "es", Text = "cargando" },
                        new TextString { Ref = "loading", IsoCode = "en", Text = "loading" },
                        new TextString { Ref = "sup_box_title", IsoCode = "en", Text = "%0 - Live Support" },
                        new TextString { Ref = "sup_box_title", IsoCode = "es", Text = "%0 - Live Support" },
                        new TextString { Ref = "view_inter_screen", IsoCode = "en", Text = "view %0 screen" },
                        new TextString { Ref = "view_inter_screen", IsoCode = "es", Text = "ver la pantalla de %0" },
                        new TextString { Ref = "cancel_screen_share", IsoCode = "en", Text = "stop viewing other's screen" },
                        new TextString { Ref = "cancel_screen_share", IsoCode = "es", Text = "dejar de ver pantalla" },
                        new TextString { Ref = "add_video", IsoCode = "en", Text = "add video" },
                        new TextString { Ref = "add_video", IsoCode = "es", Text = "añadir video" },
                        new TextString { Ref = "cancel_video", IsoCode = "en", Text = "cancel video" },
                        new TextString { Ref = "cancel_video", IsoCode = "es", Text = "cancelar video" },
                        new TextString { Ref = "share_question", IsoCode = "en", Text = "%0 wants to see your screen. Do you want to allow it?" },
                        new TextString { Ref = "share_question", IsoCode = "es", Text = "%0 quiere ver su pantalla, ¿Aceptar?" },
                        new TextString { Ref = "ok", IsoCode = "es", Text = "ok" },
                        new TextString { Ref = "ok", IsoCode = "en", Text = "ok" },
                        new TextString { Ref = "no", IsoCode = "es", Text = "no" },
                        new TextString { Ref = "no", IsoCode = "en", Text = "no" },
                        new TextString { Ref = "yes", IsoCode = "es", Text = "sí" },
                        new TextString { Ref = "yes", IsoCode = "en", Text = "yes" },
                        new TextString { Ref = "video_question", IsoCode = "es", Text = "%0 Quiere añadir video a la conversación. ¿Aceptar?" },
                        new TextString { Ref = "video_question", IsoCode = "en", Text = "%0 wants to add video. Do you want accept?" },
                        new TextString { Ref = "name_question", IsoCode = "en", Text = "Please tell us your name while we find a Support Agent for you" },
                        new TextString { Ref = "name_question", IsoCode = "es", Text = "Por favor díganos su nombre mientras le buscamos un Operador de Soporte" },
                        new TextString { Ref = "meeting_name_question", IsoCode = "en", Text = "Please tell us your name while we load the meeting room" },
                        new TextString { Ref = "meeting_name_question", IsoCode = "es", Text = "Por favor díganos su nombre mientras cargamos la sala de conferencias" },
                        new TextString { Ref = "write_name", IsoCode = "en", Text = "write here your name" },
                        new TextString { Ref = "write_name", IsoCode = "es", Text = "escriba aquí su nombre" },
                        new TextString { Ref = "waiting_support", IsoCode = "en", Text = "Ok %0, we almost found your support agent, please wait a few moments" },
                        new TextString { Ref = "waiting_support", IsoCode = "es", Text = "Ok %0, casi hemos encontrado un operador de soporte, por favor espere unos instantes" },
                        new TextString { Ref = "waiting_meeting", IsoCode = "es", Text = "Ok %0, casi hemos cargado la sala de conferencias, por favor espere unos instantes" },
                        new TextString { Ref = "waiting_meeting", IsoCode = "en", Text = "Ok %0, please wait a few moments, the conference room is loading" },
                        new TextString { Ref = "can_we_assist", IsoCode = "en", Text = "Hello! can we assist you with anything?" },
                        new TextString { Ref = "can_we_assist", IsoCode = "es", Text = "¡Hola! ¿Podemos ayudarle en algo?" },
                        new TextString { Ref = "help_method_question", IsoCode = "en", Text = "How do you prefer us to help you?" },
                        new TextString { Ref = "help_method_question", IsoCode = "es", Text = "¿Como desea que le ayudemos?" },
                        new TextString { Ref = "a_option", IsoCode = "es", Text = "Opción A" },
                        new TextString { Ref = "a_option", IsoCode = "en", Text = "A option" },
                        new TextString { Ref = "b_option", IsoCode = "es", Text = "Opción B" },
                        new TextString { Ref = "b_option", IsoCode = "en", Text = "B Option" },
                        new TextString { Ref = "start_chat", IsoCode = "en", Text = "Start chat" },
                        new TextString { Ref = "start_chat", IsoCode = "es", Text = "Iniciar chat" },
                        new TextString { Ref = "send", IsoCode = "en", Text = "Send" },
                        new TextString { Ref = "send", IsoCode = "es", Text = "Enviar" },
                        new TextString { Ref = "send_message", IsoCode = "en", Text = "Send us a message" },
                        new TextString { Ref = "send_message", IsoCode = "es", Text = "Enviarnos un mensaje" },
                        new TextString { Ref = "support_question", IsoCode = "en", Text = "A client is asking for support. Do you accept starting a chat?" },
                        new TextString { Ref = "support_question", IsoCode = "es", Text = "Un cliente desea chatear con algún operador. ¿Desea proveerle soporte?" },
                        new TextString { Ref = "click_support", IsoCode = "es", Text = "HAGA CLICK PARA CHAT DE AYUDA" },
                        new TextString { Ref = "click_support", IsoCode = "en", Text = "CLICK HERE FOR LIVE SUPPORT" },
                        new TextString { Ref = "me", IsoCode = "en", Text = "Me" },
                        new TextString { Ref = "me", IsoCode = "es", Text = "Yo" },
                        new TextString { Ref = "msg_error", IsoCode = "es", Text = "error" },
                        new TextString { Ref = "msg_error", IsoCode = "en", Text = "error" },
                        new TextString { Ref = "resend", IsoCode = "en", Text = "try again" },
                        new TextString { Ref = "resend", IsoCode = "es", Text = "reenviar" },
                        new TextString { Ref = "login_title", IsoCode = "es", Text = "Accede a vizzop con tu usuario y contraseña" },
                        new TextString { Ref = "login_title", IsoCode = "en", Text = "Log in to vizzop with your user and password" },
                        new TextString { Ref = "write_user", IsoCode = "es", Text = "tu usuario" },
                        new TextString { Ref = "write_user", IsoCode = "en", Text = "your user" },
                        new TextString { Ref = "write_password", IsoCode = "es", Text = "tu contraseña" },
                        new TextString { Ref = "write_password", IsoCode = "en", Text = "your password" },
                        new TextString { Ref = "login", IsoCode = "en", Text = "log in" },
                        new TextString { Ref = "login", IsoCode = "es", Text = "acceder" },
                        new TextString { Ref = "user", IsoCode = "es", Text = "usuario" },
                        new TextString { Ref = "user", IsoCode = "en", Text = "user" },
                        new TextString { Ref = "password", IsoCode = "en", Text = "password" },
                        new TextString { Ref = "password", IsoCode = "es", Text = "contraseña" },
                        new TextString { Ref = "error_login", IsoCode = "es", Text = "No es posible acceder con ese usuario y contraseña. Por favor verifique los datos y pruebe de nuevo." },
                        new TextString { Ref = "error_login", IsoCode = "en", Text = "It''s not possible to acces with this user and password combination. Please check them and try again." },
                        new TextString { Ref = "error_sending", IsoCode = "en", Text = "not sent" },
                        new TextString { Ref = "error_sending", IsoCode = "es", Text = "no enviado" },
                        new TextString { Ref = "end_session", IsoCode = "en", Text = "Are you sure that you want to end this chat?" },
                        new TextString { Ref = "end_session", IsoCode = "es", Text = "¿Está seguro de que quiere terminar este chat?" },
                        new TextString { Ref = "chat_with", IsoCode = "en", Text = "chat with %0" },
                        new TextString { Ref = "chat_with", IsoCode = "es", Text = "chat con %0" },
                        new TextString { Ref = "interlocutor_writing", IsoCode = "en", Text = "...%0 is writing..." },
                        new TextString { Ref = "interlocutor_writing", IsoCode = "es", Text = "...%0 está escribiendo..." },
                        new TextString { Ref = "email_footer", IsoCode = "en", Text = "<br/><br/><hr/><strong>Powered by</strong> Zentralized Ltd.<br />145-157 St John Street<br />London, EC1V 4PW<br /><a href='mailto:customer.service@vizzop.com'>customer.service@vizzop.com</a> | <a href='https://vizzop.com'>vizzop.com</a><br/>Copyright 2011 Zentralized, Ltd. All Rights Reserved. Zentralized and the Zentralized logo are trademarks of Zentralized, Ltd. in the United States and other countries.All other trademarks used are the property of their respective owners." },
                        new TextString { Ref = "email_reminder_subject", IsoCode = "en", Text = @"vizzop - Password Reminder" },
                        new TextString { Ref = "email_reminder_subject", IsoCode = "es", Text = @"vizzop - Recordatorio de contraseña" },
                        new TextString { Ref = "email_reminder_contents", IsoCode = "es", Text = @"vizzop<hr/>Su usuario en vizzop es: %0.<br/>Acuda a esta URL para resetear su contraseña: <a href='%1'>%1</a>" },
                        new TextString { Ref = "email_reminder_contents", IsoCode = "en", Text = @"vizzop<hr/>Your vizzop user is: %0.<br/>Check this address to reset your password: <br/> <a href='%1'>%1</a>" },
                        new TextString { Ref = "email_account_created_subject", IsoCode = "es", Text = @"Bienvenido a vizzop" },
                        new TextString { Ref = "email_account_created_subject", IsoCode = "en", Text = @"Welcome to vizzop" },
                        new TextString { Ref = "email_account_created_contents", IsoCode = "es", Text = @"<b>%0 </b><hr/>Gracias %1!<br/><br/>Hemos creado su nueva cuenta en vizzop satisfactoriamente. Recuerde que sus credenciales de acceso son:  %2.<br/>Siempre puede acceder al sistema desde esta dirección: <br/><a href='https://vizzop.com/Account/LogOn/'>https://vizzop.com/Account/LogOn/</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_account_created_contents", IsoCode = "en", Text = @"<b>%0 </b><hr/>Thank you %1!<br/><br/>We have created your new account at vizzop successfully. Please remember that your login username and domain is:  %2.<br/>You can access the system at this address: <br/><a href='https://vizzop.com/Account/LogOn/'>https://vizzop.com/Account/LogOn/</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_review_created_subject", IsoCode = "es", Text = @"%0 - Mensaje enviado" },
                        new TextString { Ref = "email_review_created_subject", IsoCode = "en", Text = @"%0 - Message sent" },
                        new TextString { Ref = "email_review_created_contents", IsoCode = "es", Text = @"<h1><b>%0 </b></h1><br/>Gracias %1!<br/><br/>En %0 hemos recibido su mensaje: <br/><i>%2</i><br/><br/> Le contestaremos en breve.<br/><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_review_created_contents", IsoCode = "en", Text = @"<h1><b>%0 </b></h1><br/>Thank you %1!<br/><br/>Here at %0 we have received your message: <br/><i>%2</i><br/><br/> We will answer you as quick as possible.<br/><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_created_subject", IsoCode = "es", Text = @"%0 - Mensaje enviado" },
                        new TextString { Ref = "email_ticket_created_subject", IsoCode = "en", Text = @"%0 - Message sent" },
                        new TextString { Ref = "email_ticket_created_contents", IsoCode = "es", Text = @"<h1><b>%0 </b></h1><br/>Gracias %1!<br/><br/>En %0 hemos recibido su mensaje: <br/><i>%2</i><br/><br/> Le contestaremos en breve.<br/>Si desea consultar el estado de esta conversación, acuda a esta dirección: <br/><a href='%3'>%3</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_created_contents", IsoCode = "en", Text = @"<h1><b>%0 </b></h1><br/>Thank you %1!<br/><br/>Here at %0 we have received your message: <br/><i>%2</i><br/><br/> We will answer you as quick as possible.<br/>If you want to check the status of this conversation you can check this address: <br/><a href='%3'>%3</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_response_contents", IsoCode = "es", Text = @"<h1><b>%0 </b></h1><br/>Hola %1!<br/><br/>Hemos respondido a su pregunta: <b>%2</b><br/><i>%3</i><br/><br/> Para responder acuda a esta dirección: <br/><a href='%4'>%4</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_response_contents", IsoCode = "en", Text = @"<h1><b>%0 </b></h1><br/>Hello %1!<br/><br/>We've replied to your inquiry: <b>%2</b><br/><i>%3</i><br/><br/> please check this address to reply: <br/><a href='%4'>%4</a><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_response_subject", IsoCode = "es", Text = @"%0 - Respuesta a su mensaje" },
                        new TextString { Ref = "email_ticket_response_subject", IsoCode = "en", Text = @"%0 - Answer to your message" },
                        new TextString { Ref = "email_ticket_newmessage_business_subject", IsoCode = "es", Text = @"%0 - New Message from a client" },
                        new TextString { Ref = "email_ticket_newmessage_business_subject", IsoCode = "en", Text = @"%0 - Nuevo Mensaje de un cliente" },
                        new TextString { Ref = "email_ticket_newmessage_business_contents", IsoCode = "es", Text = @"<h1><b>%0 </b></h1><br/><div style='padding:5px; background-color: #f0f0f0;'><h4>Ha recibido un nuevo mensaje en su página web</h4> <br/>%1: <br/><i>%2</i></div><br/><br/><br/><br/><br/>" },
                        new TextString { Ref = "email_ticket_newmessage_business_contents", IsoCode = "en", Text = @"<h1><b>%0 </b></h1><br/><div style='padding:5px; background-color: #f0f0f0;'><h4>You have received a new message in your website</h4> <br/>%1: <br/><i>%2</i></div><br/><br/><br/><br/><br/>" },
                        new TextString { Ref = "locked_by", IsoCode = "es", Text = "Bloqueado por %0" },
                        new TextString { Ref = "locked_by", IsoCode = "en", Text = "Locked by %0" },
                        new TextString { Ref = "supporting", IsoCode = "es", Text = "ticket abierto" },
                        new TextString { Ref = "supporting", IsoCode = "en", Text = "open ticket" },
                        new TextString { Ref = "closed_ticket", IsoCode = "es", Text = "ticket cerrado" },
                        new TextString { Ref = "closed_ticket", IsoCode = "en", Text = "closed ticket" },
                        new TextString { Ref = "waiting_approval", IsoCode = "es", Text = "ticket abierto" },
                        new TextString { Ref = "waiting_approval", IsoCode = "en", Text = "open ticket" },
                        new TextString { Ref = "data_saved", IsoCode = "es", Text = "Datos guardados correctamente" },
                        new TextString { Ref = "data_saved", IsoCode = "en", Text = "Changes were correctly saved" },
                        new TextString { Ref = "reopen_ticket", IsoCode = "es", Text = "reabrir ticket" },
                        new TextString { Ref = "reopen_ticket", IsoCode = "en", Text = "re-open ticket" },
                        new TextString { Ref = "support_message_sent", IsoCode = "es", Text = "Gracias por contactar con nosotros, recibirá la respuesta muy pronto." },
                        new TextString { Ref = "support_message_sent", IsoCode = "en", Text = "Thank you for contacting with us, you will receive our response shortly." },
                        new TextString { Ref = "view_reply", IsoCode = "es", Text = "ver / responder" },
                        new TextString { Ref = "view_reply", IsoCode = "en", Text = "view / reply" },
                        new TextString { Ref = "leave_message", IsoCode = "es", Text = "<center><h3>En estos momentos todos nuestros agentes estan ocupados. <br/>Deje su mensaje.</h3></center>" },
                        new TextString { Ref = "leave_message", IsoCode = "en", Text = "<center><h3>There are no agents available to chat to you at the moment. <br/>Leave us a message.</h3></center>" },
                        new TextString { Ref = "checked_leave_message", IsoCode = "en", Text = "<center><h3>There are no agents available to chat to you at the moment. <br/>Leave us a message.</h3></center>" },
                        new TextString { Ref = "checked_leave_message", IsoCode = "es", Text = "<center><h3>En estos momentos todos nuestros agentes estan ocupados. <br/>Deje su mensaje.</h3></center>" },
                        new TextString { Ref = "name", IsoCode = "es", Text = "Nombre" },
                        new TextString { Ref = "name", IsoCode = "en", Text = "Name" },
                        new TextString { Ref = "email", IsoCode = "en", Text = "E-mail" },
                        new TextString { Ref = "email", IsoCode = "es", Text = "E-mail" },
                        new TextString { Ref = "repeat_email", IsoCode = "en", Text = "Confirm E-mail" },
                        new TextString { Ref = "repeat_email", IsoCode = "es", Text = "Confirme E-mail" },
                        new TextString { Ref = "write_email", IsoCode = "en", Text = "write here your e-mail address" },
                        new TextString { Ref = "write_email", IsoCode = "es", Text = "escriba aquí su dirección e-mail" },
                        new TextString { Ref = "confirm_email", IsoCode = "en", Text = "confirm your e-mail address" },
                        new TextString { Ref = "confirm_email", IsoCode = "es", Text = "confirme su dirección e-mail" },
                        new TextString { Ref = "message", IsoCode = "en", Text = "Message" },
                        new TextString { Ref = "message", IsoCode = "es", Text = "Mensaje" },
                        new TextString { Ref = "client_message", IsoCode = "en", Text = "write here your message" },
                        new TextString { Ref = "client_message", IsoCode = "es", Text = "escriba aquí su mensaje" },
                        new TextString { Ref = "interlocutor_closed_chat", IsoCode = "es", Text = "%0 ha salido del chat" },
                        new TextString { Ref = "interlocutor_closed_chat", IsoCode = "en", Text = "%0 has left the conversation" },
                        new TextString { Ref = "interlocutor_ended_video", IsoCode = "es", Text = "%0 ha cerrado la sesion de video" },
                        new TextString { Ref = "interlocutor_ended_video", IsoCode = "en", Text = "%0 has ended the video session" },
                        new TextString { Ref = "new_message_from", IsoCode = "en", Text = "New message from %0" },
                        new TextString { Ref = "new_message_from", IsoCode = "es", Text = "Nuevo mensaje de %0" },
                        new TextString { Ref = "anon_client", IsoCode = "en", Text = "Anonymous client" },
                        new TextString { Ref = "anon_client", IsoCode = "es", Text = "Cliente anónimo" },
                        new TextString { Ref = "screen_view", IsoCode = "en", Text = "view screen" },
                        new TextString { Ref = "screen_view", IsoCode = "es", Text = "ver pantalla" },
                        new TextString { Ref = "interlocutor_not_available", IsoCode = "en", Text = "%0 is not available now" },
                        new TextString { Ref = "interlocutor_not_available", IsoCode = "es", Text = "%0 no está disponible ahora" },
                        new TextString { Ref = "waiting_startchat", IsoCode = "en", Text = "Please wait while the chat starts" },
                        new TextString { Ref = "waiting_startchat", IsoCode = "es", Text = "Por favor espere mientras comienza el chat" },
                        new TextString { Ref = "loading_screenview", IsoCode = "en", Text = "Loading Screen" },
                        new TextString { Ref = "loading_screenview", IsoCode = "es", Text = "Cargando Pantalla" },
                        new TextString { Ref = "err_loading_screenview", IsoCode = "en", Text = "There was an error loading your interlocutor's screen" },
                        new TextString { Ref = "err_loading_screenview", IsoCode = "es", Text = "Ocurrió un error al cargar la pantalla de su interlocutor." },
                        new TextString { Ref = "chat_cancelled", IsoCode = "en", Text = "The chat session was cancelled" },
                        new TextString { Ref = "chat_cancelled", IsoCode = "es", Text = "El chat se canceló" },
                        new TextString { Ref = "wait_end_session", IsoCode = "en", Text = "Please wait while we the chat session finishes" },
                        new TextString { Ref = "wait_end_session", IsoCode = "es", Text = "Por favor espere mientras finaliza la sesión de chat" },
                        new TextString { Ref = "shareframe_description", IsoCode = "en", Text = "The contents of this window are based upon the HTML code of your interlocutor'a screen, so they may not be completely accurate." },
                        new TextString { Ref = "shareframe_description", IsoCode = "es", Text = "El contenido de esta ventana está basado en el HTML que su interlocutor está visualizando, por lo que podría no ser exacto." },
                        new TextString { Ref = "adv_statusbar", IsoCode = "en", Text = "<a href='http://vizzop.com' target='_blank'>vizzop - User Support System</a>" },
                        new TextString { Ref = "adv_statusbar", IsoCode = "es", Text = "<a href='http://vizzop.com' target='_blank'>vizzop - Sistema de Soporte a Usuarios</a>" },
                        new TextString { Ref = "loading_video", IsoCode = "en", Text = "Loading video<br/>please wait" },
                        new TextString { Ref = "loading_video", IsoCode = "es", Text = "Cargando video<br/>por favor espere" },
                        new TextString { Ref = "support_agents", IsoCode = "en", Text = "Support Agents" },
                        new TextString { Ref = "support_agents", IsoCode = "es", Text = "Agentes de Soporte" },
                        new TextString { Ref = "already_approved", IsoCode = "en", Text = "Some other Support Agent started giving support to this client already" },
                        new TextString { Ref = "already_approved", IsoCode = "es", Text = "Algun otro Agente de Soporte ya ha comenzado el chat con este cliente" },
                        new TextString { Ref = "client_on", IsoCode = "en", Text = "Client on %0" },
                        new TextString { Ref = "client_on", IsoCode = "es", Text = "Cliente en %0" },
                        new TextString { Ref = "client", IsoCode = "en", Text = "Client" },
                        new TextString { Ref = "client", IsoCode = "es", Text = "Cliente" },
                        new TextString { Ref = "steal_lock", IsoCode = "en", Text = "force unlock" },
                        new TextString { Ref = "steal_lock", IsoCode = "es", Text = "forzar desbloqueo" },
                        new TextString { Ref = "agents_available", IsoCode = "en", Text = "Agents available for chat" },
                        new TextString { Ref = "agents_available", IsoCode = "es", Text = "Agentes disponibles para soporte" },
                        new TextString { Ref = "agents_not_available", IsoCode = "en", Text = "No available agents" },
                        new TextString { Ref = "agents_not_available", IsoCode = "es", Text = "No hay agentes disponibles" },
                        new TextString { Ref = "leave_us_a_messsage", IsoCode = "en", Text = "Leave us a message" },
                        new TextString { Ref = "leave_us_a_messsage", IsoCode = "es", Text = "Déjenos un mensaje" }
                        );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                if (isocodes == true)
                {
                    context.Isocodes.AddOrUpdate(
                        new Isocode { IsoCode = "ab", Name = "Abkhazian" },
                        new Isocode { IsoCode = "aa", Name = "Afar" },
                        new Isocode { IsoCode = "af", Name = "Afrikaans" },
                        new Isocode { IsoCode = "sq", Name = "Albanian" },
                        new Isocode { IsoCode = "am", Name = "Amharic" },
                        new Isocode { IsoCode = "ar", Name = "Arabic" },
                        new Isocode { IsoCode = "an", Name = "Aragonese" },
                        new Isocode { IsoCode = "hy", Name = "Armenian" },
                        new Isocode { IsoCode = "as", Name = "Assamese" },
                        new Isocode { IsoCode = "ay", Name = "Aymara" },
                        new Isocode { IsoCode = "az", Name = "Azerbaijani" },
                        new Isocode { IsoCode = "ba", Name = "Bashkir" },
                        new Isocode { IsoCode = "eu", Name = "Basque" },
                        new Isocode { IsoCode = "bn", Name = "Bengali (Bangla)" },
                        new Isocode { IsoCode = "dz", Name = "Bhutani" },
                        new Isocode { IsoCode = "bh", Name = "Bihari" },
                        new Isocode { IsoCode = "bi", Name = "Bislama" },
                        new Isocode { IsoCode = "br", Name = "Breton" },
                        new Isocode { IsoCode = "bg", Name = "Bulgarian" },
                        new Isocode { IsoCode = "my", Name = "Burmese" },
                        new Isocode { IsoCode = "be", Name = "Byelorussian (Belarusian)" },
                        new Isocode { IsoCode = "km", Name = "Cambodian" },
                        new Isocode { IsoCode = "ca", Name = "Catalan" },
                        new Isocode { IsoCode = "zh", Name = "Chinese" },
                        new Isocode { IsoCode = "co", Name = "Corsican" },
                        new Isocode { IsoCode = "hr", Name = "Croatian" },
                        new Isocode { IsoCode = "cs", Name = "Czech" },
                        new Isocode { IsoCode = "da", Name = "Danish" },
                        new Isocode { IsoCode = "nl", Name = "Dutch" },
                        new Isocode { IsoCode = "en", Name = "English" },
                        new Isocode { IsoCode = "eo", Name = "Esperanto" },
                        new Isocode { IsoCode = "et", Name = "Estonian" },
                        new Isocode { IsoCode = "fo", Name = "Faeroese" },
                        new Isocode { IsoCode = "fa", Name = "Farsi" },
                        new Isocode { IsoCode = "fj", Name = "Fiji" },
                        new Isocode { IsoCode = "fi", Name = "Finnish" },
                        new Isocode { IsoCode = "fr", Name = "French" },
                        new Isocode { IsoCode = "fy", Name = "Frisian" },
                        new Isocode { IsoCode = "gl", Name = "Galician" },
                        new Isocode { IsoCode = "gd", Name = "Gaelic (Scottish)" },
                        new Isocode { IsoCode = "gv", Name = "Gaelic (Manx)" },
                        new Isocode { IsoCode = "ka", Name = "Georgian" },
                        new Isocode { IsoCode = "de", Name = "German" },
                        new Isocode { IsoCode = "el", Name = "Greek" },
                        new Isocode { IsoCode = "kl", Name = "Greenlandic" },
                        new Isocode { IsoCode = "gn", Name = "Guarani" },
                        new Isocode { IsoCode = "gu", Name = "Gujarati" },
                        new Isocode { IsoCode = "ht", Name = "Haitian Creole" },
                        new Isocode { IsoCode = "ha", Name = "Hausa" },
                        new Isocode { IsoCode = "he", Name = "Hebrew" },
                        new Isocode { IsoCode = "iw", Name = "Hebrew" },
                        new Isocode { IsoCode = "hi", Name = "Hindi" },
                        new Isocode { IsoCode = "hu", Name = "Hungarian" },
                        new Isocode { IsoCode = "is", Name = "Icelandic" },
                        new Isocode { IsoCode = "io", Name = "Ido" },
                        new Isocode { IsoCode = "id", Name = "Indonesian" },
                        new Isocode { IsoCode = "in", Name = "Indonesian" },
                        new Isocode { IsoCode = "ia", Name = "Interlingua" },
                        new Isocode { IsoCode = "ie", Name = "Interlingue" },
                        new Isocode { IsoCode = "iu", Name = "Inuktitut" },
                        new Isocode { IsoCode = "ik", Name = "Inupiak" },
                        new Isocode { IsoCode = "ga", Name = "Irish" },
                        new Isocode { IsoCode = "it", Name = "Italian" },
                        new Isocode { IsoCode = "ja", Name = "Japanese" },
                        new Isocode { IsoCode = "jv", Name = "Javanese" },
                        new Isocode { IsoCode = "kn", Name = "Kannada" },
                        new Isocode { IsoCode = "ks", Name = "Kashmiri" },
                        new Isocode { IsoCode = "kk", Name = "Kazakh" },
                        new Isocode { IsoCode = "rw", Name = "Kinyarwanda (Ruanda)" },
                        new Isocode { IsoCode = "ky", Name = "Kirghiz" },
                        new Isocode { IsoCode = "rn", Name = "Kirundi (Rundi)" },
                        new Isocode { IsoCode = "ko", Name = "Korean" },
                        new Isocode { IsoCode = "ku", Name = "Kurdish" },
                        new Isocode { IsoCode = "lo", Name = "Laothian" },
                        new Isocode { IsoCode = "la", Name = "Latin" },
                        new Isocode { IsoCode = "lv", Name = "Latvian (Lettish)" },
                        new Isocode { IsoCode = "li", Name = "Limburgish ( Limburger)" },
                        new Isocode { IsoCode = "ln", Name = "Lingala" },
                        new Isocode { IsoCode = "lt", Name = "Lithuanian" },
                        new Isocode { IsoCode = "mk", Name = "Macedonian" },
                        new Isocode { IsoCode = "mg", Name = "Malagasy" },
                        new Isocode { IsoCode = "ms", Name = "Malay" },
                        new Isocode { IsoCode = "ml", Name = "Malayalam" },
                        new Isocode { IsoCode = "mt", Name = "Maltese" },
                        new Isocode { IsoCode = "mi", Name = "Maori" },
                        new Isocode { IsoCode = "mr", Name = "Marathi" },
                        new Isocode { IsoCode = "mo", Name = "Moldavian" },
                        new Isocode { IsoCode = "mn", Name = "Mongolian" },
                        new Isocode { IsoCode = "na", Name = "Nauru" },
                        new Isocode { IsoCode = "ne", Name = "Nepali" },
                        new Isocode { IsoCode = "no", Name = "Norwegian" },
                        new Isocode { IsoCode = "oc", Name = "Occitan" },
                        new Isocode { IsoCode = "or", Name = "Oriya" },
                        new Isocode { IsoCode = "om", Name = "Oromo (Afaan Oromo)" },
                        new Isocode { IsoCode = "ps", Name = "Pashto (Pushto)" },
                        new Isocode { IsoCode = "pl", Name = "Polish" },
                        new Isocode { IsoCode = "pt", Name = "Portuguese" },
                        new Isocode { IsoCode = "pa", Name = "Punjabi" },
                        new Isocode { IsoCode = "qu", Name = "Quechua" },
                        new Isocode { IsoCode = "rm", Name = "Rhaeto-Romance" },
                        new Isocode { IsoCode = "ro", Name = "Romanian" },
                        new Isocode { IsoCode = "ru", Name = "Russian" },
                        new Isocode { IsoCode = "sm", Name = "Samoan" },
                        new Isocode { IsoCode = "sg", Name = "Sangro" },
                        new Isocode { IsoCode = "sa", Name = "Sanskrit" },
                        new Isocode { IsoCode = "sr", Name = "Serbian" },
                        new Isocode { IsoCode = "sh", Name = "Serbo-Croatian" },
                        new Isocode { IsoCode = "st", Name = "Sesotho" },
                        new Isocode { IsoCode = "tn", Name = "Setswana" },
                        new Isocode { IsoCode = "sn", Name = "Shona" },
                        new Isocode { IsoCode = "ii", Name = "Sichuan Yi" },
                        new Isocode { IsoCode = "sd", Name = "Sindhi" },
                        new Isocode { IsoCode = "si", Name = "Sinhalese" },
                        new Isocode { IsoCode = "ss", Name = "Siswati" },
                        new Isocode { IsoCode = "sk", Name = "Slovak" },
                        new Isocode { IsoCode = "sl", Name = "Slovenian" },
                        new Isocode { IsoCode = "so", Name = "Somali" },
                        new Isocode { IsoCode = "es", Name = "Spanish" },
                        new Isocode { IsoCode = "su", Name = "Sundanese" },
                        new Isocode { IsoCode = "sw", Name = "Swahili (Kiswahili)" },
                        new Isocode { IsoCode = "sv", Name = "Swedish" },
                        new Isocode { IsoCode = "tl", Name = "Tagalog" },
                        new Isocode { IsoCode = "tg", Name = "Tajik" },
                        new Isocode { IsoCode = "ta", Name = "Tamil" },
                        new Isocode { IsoCode = "tt", Name = "Tatar" },
                        new Isocode { IsoCode = "te", Name = "Telugu" },
                        new Isocode { IsoCode = "th", Name = "Thai" },
                        new Isocode { IsoCode = "bo", Name = "Tibetan" },
                        new Isocode { IsoCode = "ti", Name = "Tigrinya" },
                        new Isocode { IsoCode = "to", Name = "Tonga" },
                        new Isocode { IsoCode = "ts", Name = "Tsonga" },
                        new Isocode { IsoCode = "tr", Name = "Turkish" },
                        new Isocode { IsoCode = "tk", Name = "Turkmen" },
                        new Isocode { IsoCode = "tw", Name = "Twi" },
                        new Isocode { IsoCode = "ug", Name = "Uighur" },
                        new Isocode { IsoCode = "uk", Name = "Ukrainian" },
                        new Isocode { IsoCode = "ur", Name = "Urdu" },
                        new Isocode { IsoCode = "uz", Name = "Uzbek" },
                        new Isocode { IsoCode = "vi", Name = "Vietnamese" },
                        new Isocode { IsoCode = "vo", Name = "Volapük" },
                        new Isocode { IsoCode = "wa", Name = "Wallon" },
                        new Isocode { IsoCode = "cy", Name = "Welsh" },
                        new Isocode { IsoCode = "wo", Name = "Wolof" },
                        new Isocode { IsoCode = "xh", Name = "Xhosa" },
                        new Isocode { IsoCode = "yi", Name = "Yiddish" },
                        new Isocode { IsoCode = "ji", Name = "Yiddish" },
                        new Isocode { IsoCode = "yo", Name = "Yoruba" },
                        new Isocode { IsoCode = "zu", Name = "Zulu" }
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }


            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_Isocodes_IsoCode ON Isocodes (IsoCode)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_Sales_Hash ON Sales (Hash)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_BusinesClients_Email ON businesses (Email)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_BusinesClients_ApiKey ON businesses (ApiKey)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_BusinesClients_Domain ON businesses (Domain)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_MeetingSessions_Name ON MeetingSessions (Name)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_Conversers_UserName_BusinessID ON Conversers (UserName, business_ID)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_TextStrings_ID_IsoCode ON TextStrings (Ref, IsoCode)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_ServiceTypes_Name ON ServiceTypes (Name)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_ServiceTypes_ZIndex ON ServiceTypes (ZIndex)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_Countries_CountryName ON Countries (CountryName)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_Countries_CountryCode ON Countries (CountryCode)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }
            try
            {

                if (countries == true)
                {
                    context.Countries.AddOrUpdate(
                        new Country { CountryName = "AFGHANISTAN", CountryCode = "AF", CountryPrefix = "" },
                        new Country { CountryName = "ALAND ISLANDS", CountryCode = "AX", CountryPrefix = "" },
                        new Country { CountryName = "ALBANIA", CountryCode = "AL", CountryPrefix = "" },
                        new Country { CountryName = "ALGERIA", CountryCode = "DZ", CountryPrefix = "" },
                        new Country { CountryName = "AMERICAN SAMOA", CountryCode = "AS", CountryPrefix = "" },
                        new Country { CountryName = "ANDORRA", CountryCode = "AD", CountryPrefix = "" },
                        new Country { CountryName = "ANGOLA", CountryCode = "AO", CountryPrefix = "" },
                        new Country { CountryName = "ANGUILLA", CountryCode = "AI", CountryPrefix = "" },
                        new Country { CountryName = "ANTARCTICA", CountryCode = "AQ", CountryPrefix = "" },
                        new Country { CountryName = "ANTIGUA AND BARBUDA", CountryCode = "AG", CountryPrefix = "" },
                        new Country { CountryName = "ARGENTINA", CountryCode = "AR", CountryPrefix = "" },
                        new Country { CountryName = "ARMENIA", CountryCode = "AM", CountryPrefix = "" },
                        new Country { CountryName = "ARUBA", CountryCode = "AW", CountryPrefix = "" },
                        new Country { CountryName = "AUSTRALIA", CountryCode = "AU", CountryPrefix = "" },
                        new Country { CountryName = "AUSTRIA", CountryCode = "AT", CountryPrefix = "" },
                        new Country { CountryName = "AZERBAIJAN", CountryCode = "AZ", CountryPrefix = "" },
                        new Country { CountryName = "BAHAMAS", CountryCode = "BS", CountryPrefix = "" },
                        new Country { CountryName = "BAHRAIN", CountryCode = "BH", CountryPrefix = "" },
                        new Country { CountryName = "BANGLADESH", CountryCode = "BD", CountryPrefix = "" },
                        new Country { CountryName = "BARBADOS", CountryCode = "BB", CountryPrefix = "" },
                        new Country { CountryName = "BELARUS", CountryCode = "BY", CountryPrefix = "" },
                        new Country { CountryName = "BELGIUM", CountryCode = "BE", CountryPrefix = "" }
                        );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {

                if (servicetypes == true)
                {
                    context.ServiceTypes.AddOrUpdate(
                        new ServiceType { Name = "Free - 1 Agent included", ZIndex = 0, CartName = "0", Price = "0.00", Type = "version" },
                        new ServiceType { Name = "Standard ($10/month) - 3 Agents included", ZIndex = 1, CartName = "1", Price = "10.00", Type = "version" },
                        /*new ServiceType { Name = "Premium Version - $59/month", ZIndex = 2, CartName = "2", Price = "59.00", Type = "version" },*/
                        /*new ServiceType { Name = "Beta Version - Free", ZIndex = 2, CartName = "2", Price = "0.00", Type = "version" },*/
                        new ServiceType { Name = "Support Agent - $10/month", ZIndex = 2, CartName = "2", Price = "10.00", Type = "agent" },
                        /*new ServiceType { Name = "Premium Agent - $20/month", ZIndex = 4, CartName = "4", Price = "20.00", Type = "agent" },*/
                        new ServiceType { Name = "Upgrade from Free to Standard Version - $10", ZIndex = 3, CartName = "3", Price = "10.00", Type = "upgrade" }
                        );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Utils.GrabaLog(Utils.NivelLog.info, e.Message); 
            }

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
