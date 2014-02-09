using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ScreenCapture
    {

        public ScreenCapture()
        {
            this.GUID = Guid.NewGuid().ToString();
            this.CreatedOn = DateTime.UtcNow;
        }

        [NonSerialized]
        public Utils utils = new Utils();

        public int ID { get; set; }

        [Required]
        public virtual Converser converser { get; set; }

        /*[Required]*/
        [Column(TypeName = "ntext")]
        [MaxLength]
        public string Data { get; set; }

        [Column(TypeName = "ntext")]
        [MaxLength]
        public string Blob { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int ScrollLeft { get; set; }

        [Required]
        public int ScrollTop { get; set; }

        [Required]
        public int MouseX { get; set; }

        [Required]
        public int MouseY { get; set; }

        [StringLength(8000)]
        public string Headers { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public string GUID { get; set; }

        public Boolean AddToCache()
        {
            try
            {

                /*
                #if DEBUG
                            foreach (var obj in cache.GetSystemRegions().SelectMany(cache.GetObjectsInRegion))
                            {
                                cache.ResetObjectTimeout(obj.Key, new TimeSpan(0, 0, 0, 0, 01)); //set to expire basically immediately
                            }
                #endif
                */

                Boolean result = false;
                int counter = 0;
                this.ID = 0;
                while ((result == false) && (counter < 10))
                {
                    result = this.DoAddToCache();
                }
                return result;
            }
            catch (Exception ex)
            {
                Utils utils = new Utils();
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public Boolean DoAddToCache()
        {
            try
            {

                List<Message> Messages = new List<Message>();
                this.utils = null;
                string key = "screenshot_from_" + this.converser.UserName + "@" + this.converser.Business.Domain;


                //SingletonCache.Instance.Insert(key + "_penultimate", SingletonCache.Instance.Get(key));

                /*
                 * Lo que vamos a hacer aquí es que si esta captura de pantalla no lleva imagen
                 * pero si lleva el resto (punto del ratón) vamos a tomar la imagen que ya había antes
                 */
                try
                {
                    object result = SingletonCache.Instance.Get(key);
                    if (result != null)
                    {
                        ScreenCapture return_sc = (ScreenCapture)result;
                        if (this.CreatedOn < return_sc.CreatedOn)
                        {
                            //Solo metemos en cache lo mas fresquito...
                            return true;
                        }
                        if ((this.Data == null) || (this.Data == ""))
                        {
                            if ((return_sc.Data != null) || (return_sc.Data == ""))
                            {
                                this.Data = return_sc.Data;
                            }
                        }
                        if (this.Blob == null)
                        {
                            if (return_sc.Blob != null)
                            {
                                this.Blob = return_sc.Blob;
                            }
                        }
                    }
                }
                catch (Exception _ex)
                {
                    this.utils = new Utils();
                    this.utils.GrabaLogExcepcion(_ex);
                }

                //Solo metemos en caché las capturas con imagen o blob asociados....
                if ((this.Data != null) || (this.Data != "") || (this.Blob != null))
                {
/*
#if DEBUG
                    this.utils = new Utils();
                    utils.GrabaLog(Utils.NivelLog.info, " Imagen ingresada a cache");
#endif
*/
                    return SingletonCache.Instance.Insert(key, this);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.utils = new Utils();
                this.utils.GrabaLogExcepcion(ex);
                this.utils.GrabaLog(Utils.NivelLog.error, "No se pudo grabar sc: " + this.converser + "@" + this.converser.Business);
                return false;
            }
        }

    }
}