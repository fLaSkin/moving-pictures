using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Linq;
using System.Web;
using System.Net;
using System.Threading;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Text.RegularExpressions;
using Cornerstone.Tools.Translate;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources;
using Cornerstone.Extensions;
using System.Globalization;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("movie_info")]
    public class DBMovieInfo: MovingPicturesDBTable, IComparable, IAttributeOwner {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DBMovieInfo()
            : base() {
            this.LocalMedia.Changed += new ChangedEventHandler(LocalMedia_Changed);
        }

        void LocalMedia_Changed(object sender, EventArgs e) {
            this.LocalMedia.Sort(new DBLocalMediaComparer());
        }

        public override void AfterDelete() {
            if (ID == null) {
                while (AlternateCovers.Count > 0)
                    this.DeleteCurrentCover();
                foreach (var wh in this.WatchedHistory) {
                    wh.Delete();
                }
            }
        }

        #region Database Fields

        [DBField]
        public string Title {
            get { return _title; }
            set { 
                _title = value;
                PopulateSortBy();
                commitNeeded = true;
            }
        } private string _title;

        [DBField(FieldName = "alternate_titles", AllowDynamicFiltering = false)]
        public StringList AlternateTitles {
          get { return _alternateTitles; }

          set {
            _alternateTitles = value;
            commitNeeded = true;
          }
        } private StringList _alternateTitles;

        [DBField(AllowDynamicFiltering = false)]
        public string SortBy {
            get {
                if (_sortBy.Trim().Length == 0)
                    PopulateSortBy();

                return _sortBy; 
            }

            set {
                _sortBy = value;
                commitNeeded = true;
            }
        } private string _sortBy;
        
        [DBField]
        public StringList Directors {
            
            get { return _directors; }
            set { 
                _directors = value;
                commitNeeded = true;
            }

        } private StringList _directors;

        [DBField]
        public StringList Writers {
            get { return _writers; }

            set {
                _writers = value;
                commitNeeded = true;
            }
        } private StringList _writers;

        [DBField]
        public StringList Actors {
            get { return _actors; }

            set {
                _actors = value;
                commitNeeded = true;
            }
        } private StringList _actors;

        [DBField(FieldName = "release_date")]
        public DateTime ReleaseDate
        {
            get 
            { 
                return _releaseDate; 
            }
            set
            {
                _releaseDate = value;
                commitNeeded = true;
            }
        } private DateTime _releaseDate;

        [DBField]
        public int Year {
            get {
                if (_year == 0 && _releaseDate != null)
                    return _releaseDate.Year;

                return _year; 
            }

            set {
                _year = value;
                commitNeeded = true;
            }
        } private int _year;

        [DBField]
        public StringList Genres {
            get { return _genres; }

            set {
                _genres = value;
                commitNeeded = true;
            }
        } private StringList _genres;

        [DBField]
        public string Certification {
            get { return _certification; }

            set {
                _certification = value;
                commitNeeded = true;
            }
        } private string _certification;
             
        [DBField(AllowManualFilterInput=true)]
        public string Language {
            get { return _language; }

            set {
                _language = value;
                commitNeeded = true;
            }
        } private string _language;

        [DBField(AllowDynamicFiltering = false)]
        public string Tagline {
            get { return _tagline; }

            set {
                _tagline = value;
                commitNeeded = true;
            }
        } private string _tagline;

        [DBField(AllowDynamicFiltering = false)]
        public string Summary {
            get { return _summary; }

            set {
                _summary = value;
                commitNeeded = true;
            }
        } private string _summary;

        [DBField]
        public StringList Studios {
            get { return _studios; }

            set {
                _studios = value;
                commitNeeded = true;
            }
        } private StringList _studios;

        [DBField]
        public StringList Collections {
          get { return _collections; }

          set {
              _collections = value;
            commitNeeded = true;
          }
        } private StringList _collections;

        [DBField]
        public StringList PlotKeywords
        {
            get { return _plotKeywords; }

            set
            {
                _plotKeywords = value;
                commitNeeded = true;
            }
        } private StringList _plotKeywords;

        [DBField(AllowDynamicFiltering = false)]
        public float Score {
            get { return _score; }

            set {
                _score = value;
                while (_score > 10)
                    _score /= 10;

                _score = (float) Math.Round(_score, 1);

                commitNeeded = true;
            }
        } private float _score;

        [DBField(AllowDynamicFiltering = false)]
        public int Popularity {
            get { return _popularity; }

            set {
                _popularity = value;
                commitNeeded = true;
            }
        } private int _popularity;

        [DBField(AllowAutoUpdate = false, FieldName = "date_added")]
        public DateTime DateAdded {
            get { return _dateAdded; }

            set {
                _dateAdded = value;
                commitNeeded = true;
            }
        } private DateTime _dateAdded;

        [DBField(AllowDynamicFiltering = false)]
        public int Runtime {
            get { return _runtime; }

            set {
                _runtime = value;
                commitNeeded = true;
            }
        } private int _runtime;

        [DBField(FieldName = "imdb_id", AllowDynamicFiltering = false)]
        public string ImdbID {
            get { return _imdbID; }

            set {
                _imdbID = value;
                commitNeeded = true;
            }
        } private string _imdbID;

        [DBRelation(AutoRetrieve=true)]
        public RelationList<DBMovieInfo, DBLocalMedia> LocalMedia {
            get {
                if (_localMedia == null) {
                    _localMedia = new RelationList<DBMovieInfo, DBLocalMedia>(this);
                }
                return _localMedia; 
            }
        } RelationList<DBMovieInfo, DBLocalMedia> _localMedia;

        [DBFieldAttribute(AllowAutoUpdate = false, AllowDynamicFiltering = false)]
        public string OriginalDirectoryName {
            get {
                if (_originalDirectoryName == " ")
                    return String.Empty;

                return _originalDirectoryName;
            }

            set {
                _originalDirectoryName = value;
                commitNeeded = true;
            }
        } private string _originalDirectoryName;

        [DBRelation(AutoRetrieve = true, Filterable=false)]
        public RelationList<DatabaseTable, DBAttribute> Attributes {
            get {
                if (_attributes == null) {
                    _attributes = new RelationList<DatabaseTable, DBAttribute>(this);
                }
                return _attributes;
            }
        } RelationList<DatabaseTable, DBAttribute> _attributes;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBMovieInfo, DBUserMovieSettings> UserSettings {
          get {
            if (_userSettings == null) {
              _userSettings = new RelationList<DBMovieInfo, DBUserMovieSettings>(this);
            }
            return _userSettings;
          }
        } RelationList<DBMovieInfo, DBUserMovieSettings> _userSettings;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBMovieInfo, DBSourceMovieInfo> SourceMovieInfo {
          get {
            if (_sourceIDs == null) {
                _sourceIDs = new RelationList<DBMovieInfo, DBSourceMovieInfo>(this);
            }
            return _sourceIDs;
          }
        } RelationList<DBMovieInfo, DBSourceMovieInfo> _sourceIDs;

        [DBField(FieldName = "primary_source", Filterable = false, AllowAutoUpdate = false, Default=null)]
        public DBSourceInfo PrimarySource {
            get { return _primarySource; }

            set {
                _primarySource = value;
                PopulateSortBy();

                commitNeeded = true;
            }
        } private DBSourceInfo _primarySource;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBMovieInfo, DBWatchedHistory> WatchedHistory {
            get {
                if (_watchedHistory == null) {
                    _watchedHistory = new RelationList<DBMovieInfo, DBWatchedHistory>(this);
                }
                return _watchedHistory;
            }
        } RelationList<DBMovieInfo, DBWatchedHistory> _watchedHistory;

        public DBSourceMovieInfo GetSourceMovieInfo(int scriptID) {
            return DBSourceMovieInfo.GetOrCreate(this, scriptID);
        }

        public DBSourceMovieInfo GetSourceMovieInfo(DBSourceInfo source) {
            return DBSourceMovieInfo.GetOrCreate(this, source);
        }

        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public StringList AlternateCovers {
            get { return _covers; }

            set {
                _covers = value;
                commitNeeded = true;
            }
        } private StringList _covers;

        [DBField(AllowAutoUpdate = false, Filterable=true)]
        public String CoverFullPath
        {
            get {
                if (_coverFullPath.Trim().Length == 0 && AlternateCovers.Count > 0)
                    _coverFullPath = AlternateCovers[0];
                return _coverFullPath; 
            }

            set {
                _coverFullPath = value;
                _coverThumbFullPath = "";
                commitNeeded = true;
            }
        } private String _coverFullPath;

        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public String CoverThumbFullPath {
            get {
                if (_coverThumbFullPath.Trim().Length == 0)
                    GenerateThumbnail();
                return _coverThumbFullPath;
            }

            set {
                _coverThumbFullPath = value;
                commitNeeded = true;
            }
        } private String _coverThumbFullPath;

        [DBField(AllowAutoUpdate = false, Filterable=true)]
        public String BackdropFullPath {
            get {
                return _backdropFullPath;
            }

            set {
                _backdropFullPath = value;
                commitNeeded = true;
            }
        } private string _backdropFullPath;

        [DBField(FieldName="details_url", Filterable=false)]
        public String DetailsURL {
            get {
                return _detailsUrl;
            }

            set {
                _detailsUrl = value;
                commitNeeded = true;
            }
        } private string _detailsUrl;

        public DBUserMovieSettings ActiveUserSettings {
            get {
                return UserSettings[0];
            }
        }

        #endregion

        #region General Management Methods
        
        // deletes this movie from the database and sets all related DBLocalMedia to ignored
        public void DeleteAndIgnore() {
            foreach (DBLocalMedia currFile in LocalMedia) {
                currFile.Ignored = true;
                currFile.Commit();
            }

            Delete();
        }

        /// <summary>
        /// The total runtime of the the localmedia files in milliseconds. 
        /// Returns 0 if the data has not yet been set in all of the dblocalmedia objects
        /// </summary>
        public int ActualRuntime {
            get {
                if (actualRuntime == 0) {
                    foreach (DBLocalMedia currFile in LocalMedia) {
                        if (currFile.Duration == 0) {
                            actualRuntime = 0;
                            return 0;
                        }

                        actualRuntime += currFile.Duration;
                    }

                }
                return actualRuntime;
            }
        } 
        private int actualRuntime = 0;

        /// <summary>
        /// Returns percentage watched
        /// </summary>
        /// <param name="part">part number</param>
        /// <param name="time">time in seconds</param>
        /// <returns>the percentage 1 - 100 through the whole movie.</returns>
        public int GetPercentage(int part, int time) {
            if (part > LocalMedia.Count)
                return 0;
            
            // convert to milliseconds
            float tally = time * 1000;

            for (int i = 0; i < part-1; i++) {
                if (LocalMedia[i].Duration == 0)
                    return 0;

                tally += LocalMedia[i].Duration;
            }

            return (int) (100 * (tally / (float) ActualRuntime));
        }


        /// <summary>
        /// Deletes all of the video files associated with this movie from disk
        /// If the movie is stored in a dedicated folder, this will delete
        /// the folder.
        /// </summary>
        /// <returns>true for success, false for failure</returns>
        public bool DeleteFiles() {
            try {
                FileInfo fInfo = this.LocalMedia[0].File;
                bool isFolderDedicated = Utility.isFolderDedicated(fInfo.Directory, this.LocalMedia.Count);

                if (isFolderDedicated) {
                    Utility.GetMovieBaseDirectory(fInfo.Directory).Delete(true);
                }
                else {
                    foreach (DBLocalMedia item in this.LocalMedia) {
                        File.Delete(item.FullPath);
                    }

                    File.Delete(fInfo.Name.Replace(fInfo.Extension, ".nfo"));
                }

                this.Delete();
                return true;
            }
            catch (Exception ex) {
                logger.LogException(LogLevel.Error, "Error when deleting file", ex);
                return false;
            }
        }

        #endregion

        #region Coverart Management Methods

        // rotates the selected cover art to the next available cover
        public void NextCover() {
            if (AlternateCovers.Count <= 1)
                return;

            int index = AlternateCovers.IndexOf(CoverFullPath) + 1;
            if (index >= AlternateCovers.Count)
                index = 0;

            CoverFullPath = AlternateCovers[index];
            commitNeeded = true;
        }

        // rotates the selected cover art to the previous available cover
        public void PreviousCover() {
            if (AlternateCovers.Count <= 1)
                return;

            int index = AlternateCovers.IndexOf(CoverFullPath) - 1;
            if (index < 0)
                index = AlternateCovers.Count - 1;

            CoverFullPath = AlternateCovers[index];
            commitNeeded = true;
        }

        // removes the current cover from the selection list and deletes it and it's thumbnail 
        // from disk
        public void DeleteCurrentCover() {
            if (AlternateCovers.Count == 0)
                return;

            string coverFilePath = CoverFullPath;
            string coverThumbFilePath = CoverThumbFullPath;

            // delete thumbnail
            if (coverThumbFilePath.Trim().Length > 0) {
                FileInfo thumbFile = new FileInfo(coverThumbFilePath);
                if (thumbFile.Exists) {
                    try {
                        thumbFile.Delete();
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;
                    }
                }
            }

            // delete cover
            if (coverFilePath.Trim().Length > 0) {
                FileInfo coverFile = new FileInfo(coverFilePath);
                if (coverFile.Exists) {
                    try {
                        coverFile.Delete();
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;
                    }
                }
            }

            CoverFullPath = "";
            AlternateCovers.Remove(coverFilePath);
            commitNeeded = true;
        }

        public bool AddCoverFromFile(string filename) {
            int minWidth = MovingPicturesCore.Settings.MinimumCoverWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumCoverHeight;
            string artFolder = MovingPicturesCore.Settings.CoverArtFolder;

            Image newCover = null;
            try {
                newCover = Image.FromFile(filename);
            }
            catch (OutOfMemoryException) {
                logger.Debug("Invalid image or image format not supported: " + filename);
            }
            catch (FileNotFoundException) {
                logger.Debug("File not found: " + filename);
            }

            if (newCover == null) {
                logger.Error("Failed loading cover artwork for '" + Title + "' [" + ID + "] from " + filename + ".");
                return false;
            }
            
            // check if the image file is already in the cover folder
            FileInfo newFile = new FileInfo(filename);
            bool alreadyInFolder = newFile.Directory.FullName.Equals(new DirectoryInfo(artFolder).FullName);

            // if the file isnt in the cover folder, generate a name and save it there
            if (!alreadyInFolder) {
                string safeName = Title.Replace(' ', '.').ToValidFilename() + "-" + Year;
                string newFileName = artFolder + "\\{" + safeName + "} [" + filename.GetHashCode() + "].jpg";
                
                if (!File.Exists(newFileName) && saveImage(newFileName, newCover)) {
                    AlternateCovers.Add(newFileName);
                    commitNeeded = true;
                }
                else
                    return false;
            }

            // if it's already in the folder, just store the filename in the db
            else {
                if (!AlternateCovers.Contains(filename)) {
                    AlternateCovers.Add(filename);
                    commitNeeded = true;
                }
                else
                    return false;
            }

            // create a thumbnail for the cover
            newCover.Dispose();
            commitNeeded = true;
            GenerateThumbnail();
            logger.Info("Added cover art for '" + Title + "' from: " + filename);
            
            return true;
        }

        // Attempts to load cover art for this movie from a given URL. Optionally
        // ignores minimum resolution restrictions
        public ImageLoadResults AddCoverFromURL(string url, bool ignoreRestrictions) {
            ImageLoadResults status;
            Cover newCover = Cover.FromUrl(this, url, ignoreRestrictions, out status);

            if (status != ImageLoadResults.SUCCESS && status != ImageLoadResults.SUCCESS_REDUCED_SIZE)
                return status;

            AlternateCovers.Add(newCover.Filename);
            GenerateThumbnail();
            commitNeeded = true;
            return ImageLoadResults.SUCCESS;
        }

        // Attempts to load cover art for this movie from a given URL. Honors 
        // minimum resolution restrictions
        public ImageLoadResults AddCoverFromURL(string url) {
            return AddCoverFromURL(url, false);
        }

        public ImageLoadResults AddBackdropFromURL(string url, bool ignoreRestrictions) {
            ImageLoadResults status;
            Backdrop newBackdrop = Backdrop.FromUrl(this, url, ignoreRestrictions, out status);

            if (status != ImageLoadResults.SUCCESS && status != ImageLoadResults.SUCCESS_REDUCED_SIZE)
                return status;

            // save the backdrop
            _backdropFullPath = newBackdrop.Filename;
            commitNeeded = true;
            return ImageLoadResults.SUCCESS;
        }

        public ImageLoadResults AddBackdropFromURL(string url) {
            return AddBackdropFromURL(url, false);
        }

        public bool AddBackdropFromFile(string filename) {
            int minWidth = MovingPicturesCore.Settings.MinimumBackdropWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumBackdropHeight;
            string artFolder = MovingPicturesCore.Settings.BackdropFolder;

            Image newBackdrop = null;
            try {
                newBackdrop = Image.FromFile(filename);
            }
            catch (OutOfMemoryException e) {
                logger.DebugException("Invalid image or image format not supported.", e);
            }
            catch (FileNotFoundException e) {
                logger.DebugException("File not found.", e);
            }
            
            if (newBackdrop == null) {
                logger.Error("Failed loading backdrop for '" + Title + "' [" + ID + "] from " + filename + ".");
                return false;
            }


            // check if the image file is already in the backdrop folder
            FileInfo newFile = new FileInfo(filename);
            bool alreadyInFolder = newFile.Directory.FullName.Equals(new DirectoryInfo(artFolder).FullName);

            // if the file isnt in the backdrop folder, generate a name and save it there
            if (!alreadyInFolder) {
                string safeName = Title.Replace(' ', '.').ToValidFilename() + "-" + Year;
                string newFileName = artFolder + "\\{" + safeName + "} [" + filename.GetHashCode() + "].jpg";
                
                // save the backdrop
                bool saved = saveImage(newFileName, newBackdrop);
                if (saved) {
                    _backdropFullPath = newFileName;
                    commitNeeded = true;
                    logger.Info("Added backdrop for '" + Title + "' from: " + newFileName);
                    return true;
                }
                else 
                    return false;
            }

            // if it's already in the folder, just store the filename in the db
            else {
                newBackdrop.Dispose();
                _backdropFullPath = filename;
                commitNeeded = true;
                logger.Info("Added backdrop for '" + Title + "' from: " + filename);
                return true;
            }
        }

        private bool saveImage(string filename, Image image) {
            try {
                // try to save as a JPG
                image.Save(filename, ImageFormat.Jpeg);
                return true;
            }
            catch (ArgumentNullException) {
                logger.Debug("Error while trying to save backdrop: filename is NULL.");
            }
            catch (ExternalException) {
                try {
                    // if JPG saving failed for some reason, delete and try to resave as a PNG
                    if (File.Exists(filename))
                        File.Delete(filename);

                    logger.Warn("Failed to save file as JPG, trying PNG: " + filename);
                    image.Save(filename, ImageFormat.Png);
                    return true;
                }
                catch (Exception ex) {
                    // we are getting no where...
                    logger.Error("Error trying to save image file: " + filename, ex);
                    if (File.Exists(filename))
                        File.Delete(filename);
                }
            }
            finally {
                image.Dispose();
            }

            return false;
        }

        public void GenerateThumbnail() {
            if (CoverFullPath.Trim().Length == 0)
                return;

            string thumbsFolder = MovingPicturesCore.Settings.CoverArtThumbsFolder;
            string filename = new FileInfo(CoverFullPath).Name;
            string fullname = thumbsFolder + '\\' + filename;

            if (File.Exists(fullname)) {
                _coverThumbFullPath = fullname;
                return;
            }

            Image cover = null;
            try {
                cover = Image.FromFile(CoverFullPath);
            }
            catch (OutOfMemoryException e) {
                logger.DebugException("Invalid image or image format not supported.", e);
                return;
            }
            catch (FileNotFoundException e) {
                logger.DebugException("File not found.", e);
                return;
            }

            if (cover == null) {
                logger.Error("Error while trying to create thumbnail.");
                return;
            }

            int width = MovingPicturesCore.Settings.ThumbnailWidth;
            if (width > 1920) width = 1920;
            int height = (int)(cover.Height * ((float)width / (float)cover.Width));

            Image coverThumb = cover.GetThumbnailImage(width, height, null, IntPtr.Zero);
            if (saveImage(fullname, coverThumb)) {
                _coverThumbFullPath = fullname;
                commitNeeded = true;
            }

            cover.Dispose();
            coverThumb.Dispose();
        }

        #endregion

        #region Database Management Methods

        public static DBMovieInfo Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBMovieInfo>(id);
        }

        public static List<DBMovieInfo> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBMovieInfo>(null);
        }

        // this should be changed to reflectively commit all sub objects and relation lists
        // and moved down to the DatabaseTable class.
        public override void Commit() {
            if (this.ID == null) {
                base.Commit();
                commitNeeded = true;
            }

            foreach (DBSourceMovieInfo currInfo in SourceMovieInfo) {
                currInfo.Commit();
            }
        
            base.Commit();
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the Two Letter Language Code associated with the scraper used for the MovieInfo
        /// </summary>
        public string LanguageCode {
            get {
                // get movie scraper language property
                string langCode = string.Empty;
                if (PrimarySource != null && PrimarySource.Provider != null) {
                    langCode = PrimarySource.Provider.LanguageCode.ToLowerInvariant();
                }

                // if unable to get a specific language set to current locale
                if (string.IsNullOrEmpty(langCode) || langCode.ToLowerInvariant() == "various") {
                    try {
                        langCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                    }
                    catch {
                        langCode = MovingPicturesCore.Settings.DataProviderAutoLanguage;
                    }
                }

                return langCode;
            }
        }

        /// <summary>
        /// Gets the corrected display title for GUI, with or without Title Articles
        /// </summary>
        public string DisplayTitle {
            get {
                string displayTitle = Title;
                if (MovingPicturesCore.Settings.RemoveArticlesFromDisplayTitle) {
                    // grab the groups of articles by language to remove
                    var articleGroups = MovingPicturesCore.Settings.ArticleGroups;
                    if (articleGroups.Count == 0) return displayTitle;

                    // try to grab the appropriate list of articles, if one does not exit, bail
                    if (!articleGroups.ContainsKey(LanguageCode)) return displayTitle;
                    string articlesforRemoval = articleGroups[LanguageCode];

                    // remove article from the first word in title
                    var words = Regex.Split(Title.Trim(), @"\s+");
                    
                    // if there is only one word, then don't remove anything
                    if (words.Count() == 1) return displayTitle;
                    
                    // otherwise check if first word is an article
                    if (articlesforRemoval.Split('|').Contains(words.First().ToLowerInvariant())) {
                        // remove first word and join back the rest
                        displayTitle = string.Join(" ", words.Skip(1).ToArray());
                    }
                }

                return displayTitle;
            }
        }
        #endregion

        public override int CompareTo(object obj) {
            if (obj.GetType() == typeof(DBMovieInfo)) {
                return SortBy.CompareTo(((DBMovieInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Title;
        }

        public void PopulateSortBy() {
            if (RetrievalInProcess) return;
            
            // remove all non-word characters and replace them with spaces
            SortBy = Regex.Replace(_title, @"[^\w\s]", "", RegexOptions.IgnoreCase).ToLower().Trim();

            // try to remove a possible leading articles
            if (MovingPicturesCore.Settings.RemoveTitleArticles) {
                
                // grab the groups of articles by language to remove
                var articleGroups = MovingPicturesCore.Settings.ArticleGroups;
                if (articleGroups.Count == 0) return;

                // try to grab the appropriate list of articles, if one does not exit, bail
                if (!articleGroups.ContainsKey(LanguageCode)) return;
                string articlesforRemoval = articleGroups[LanguageCode];

                // remove those articles
                string[] prepositions = articlesforRemoval.Split('|');
                foreach (string currWord in prepositions) {
                    string word = currWord + " ";
                    if (_sortBy.ToLower().IndexOf(word) == 0) {
                        SortBy = _sortBy.Substring(word.Length);
                        return;
                    }
                }
            }
        }

        public bool PopulateDateAdded() {
            String dateOption = MovingPicturesCore.Settings.DateImportOption;

            if (dateOption == null)
                dateOption = "created";

            if (LocalMedia.Count == 0 || LocalMedia[0].ImportPath.GetDriveType() == DriveType.CDRom)
                dateOption = "current";

            switch (dateOption) {
                case "modified":
                    if (!LocalMedia[0].IsAvailable)
                        return false;
                    
                    DateAdded = LocalMedia[0].File.LastWriteTime;
                    break;
                case "current":
                    DateAdded = DateTime.Now;
                    break;
                default:
                    if (!LocalMedia[0].IsAvailable)
                        return false;

                    DateAdded = LocalMedia[0].File.CreationTime;
                    break;
            }

            return true;
        }

        public void Translate() {
            Translate(MovingPicturesCore.Settings.TranslationLanguage);
        }

        public void Translate(TranslatorLanguage language) {
            Translator tr = new Translator();
            tr.ToLanguage = language;

            Summary = tr.Translate(Summary);
            Tagline = tr.Translate(Tagline);

            string tmp = tr.Translate(Genres.ToString());
            Genres.Clear();
            foreach (string currGenre in tmp.Split('|'))
                if (currGenre.Trim().Length > 0)
                    Genres.Add(currGenre.Trim());
        }
    }
}
