using System;

namespace Build_BuildersIS.Models
{
    public class ObjectItem
    {
        /// <summary>
        /// Уникальный идентификатор объекта.
        /// </summary>
        public int ObjectID { get; set; }

        /// <summary>
        /// Уникальный идентификатор связанного проекта.
        /// </summary>
        public int? ProjectID { get; set; }

        /// <summary>
        /// Название объекта.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание объекта.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Изображение объекта в формате массива байтов.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Удобное представление изображения для привязки в XAML.
        /// </summary>
        public System.Windows.Media.Imaging.BitmapImage ImagePreview
        {
            get
            {
                if (ImageData == null || ImageData.Length == 0) return null;

                using (var stream = new System.IO.MemoryStream(ImageData))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
        }

        /// <summary>
        /// Переопределение метода ToString для отображения имени объекта.
        /// </summary>
        /// <returns>Имя объекта.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}