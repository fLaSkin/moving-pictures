using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("user_movie_settings")]
    class DBUserMovieSettings: MovingPicturesDBTable {

        public override void AfterDelete() {
        }

        #region Database Fields

        [DBFieldAttribute]
        public DBUser User {
            get { return user; }
            set {
                user = value;
                commitNeeded = true;
            }
        } private DBUser user;

        [DBFieldAttribute]
        public DBMovieInfo Movie {
            get { return movie; }
            set {
                movie = value;
                commitNeeded = true;
            }
        } private DBMovieInfo movie;

        // Value between 0 and 4. As in 4 stars.
        [DBFieldAttribute(FieldName = "user_rating", Default = null)]
        public int? UserRating {
            get { return _userRating; }
            set {
                _userRating = value;
                commitNeeded = true;
            }
        } private int? _userRating;


        [DBFieldAttribute(Default = "false")]
        public bool Watched {
            get { return _watched; }
            set {
                _watched = value;
                commitNeeded = true;
            }
        } private bool _watched;

        #endregion

        #region Database Management Methods

        public static DBUserMovieSettings Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBUserMovieSettings>(id);
        }

        public static List<DBUserMovieSettings> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBUserMovieSettings>(null);
        }

        #endregion
    }
}