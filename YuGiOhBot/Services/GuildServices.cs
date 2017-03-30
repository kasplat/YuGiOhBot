﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace YuGiOhBot.Services
{
    public class GuildServices
    {

        private const string DatabasePath = "Data Source=Databases/guildinformation.db";
        private const string PrefixTable = "prefixes";
        private const string MinimalTable = "minimalCardSetting";
        public ConcurrentDictionary<ulong, string> _guildPrefixes { get; private set; }
        public ConcurrentDictionary<ulong, bool> _minimalSettings { get; private set; }

        public async Task InitializeService()
        {

            _guildPrefixes = new ConcurrentDictionary<ulong, string>();
            _minimalSettings = new ConcurrentDictionary<ulong, bool>();

            using (var database = new SqliteConnection(DatabasePath))
            {

                await database.OpenAsync();

                using (SqliteCommand getPrefixes = database.CreateCommand())
                {

                    getPrefixes.CommandText = $"select * from {PrefixTable}";

                    using (SqliteDataReader dataReader = await getPrefixes.ExecuteReaderAsync())
                    {

                        int idOrd = dataReader.GetOrdinal("id");
                        int prefixOrd = dataReader.GetOrdinal("prefix");

                        while (await dataReader.ReadAsync())
                        {

                            _guildPrefixes.TryAdd(ulong.Parse(dataReader.GetString(idOrd)), dataReader.GetString(prefixOrd));

                        }

                    }

                }

                using (SqliteCommand getMinimal = database.CreateCommand())
                {

                    getMinimal.CommandText = $"select * from {PrefixTable}";

                    using (SqliteDataReader dataReader = await getMinimal.ExecuteReaderAsync())
                    {

                        int idOrd = dataReader.GetOrdinal("id");
                        int settingOrd = dataReader.GetOrdinal("prefix");

                        while (await dataReader.ReadAsync())
                        {

                            bool.TryParse(dataReader.GetString(settingOrd), out bool setting);
                            _minimalSettings.TryAdd(ulong.Parse(dataReader.GetString(idOrd)), setting);

                        }

                    }

                }

                database.Close();

            }

        }

        public async Task SetMinimal(ulong guildId, bool minimal)
        {

            bool doesExist;

            using (var minimalDatabase = new SqliteConnection(DatabasePath))
            {
                using (SqliteCommand checkExist = minimalDatabase.CreateCommand())
                {

                    checkExist.CommandText = $"select * from {MinimalTable} where id like '{guildId}'";

                    await minimalDatabase.OpenAsync();

                    doesExist = (await checkExist.ExecuteScalarAsync()) == null ? false : true;
                                        
                }

                if (doesExist)
                {

                    using (SqliteCommand updateSetting = minimalDatabase.CreateCommand())
                    {

                        updateSetting.CommandText = $"update {MinimalTable} set setting = @SETTING where id = @ID";
                        updateSetting.Parameters.AddWithValue("@SETTING", minimal.ToString());
                        updateSetting.Parameters.AddWithValue("@ID", guildId.ToString());

                        await updateSetting.ExecuteNonQueryAsync();

                    }

                }
                else
                {

                    using (SqliteCommand insertSetting = minimalDatabase.CreateCommand())
                    {

                        insertSetting.CommandText = $"insert into {MinimalTable} values (@ID, @SETTING)";
                        insertSetting.Parameters.AddWithValue("@SETTING", minimal.ToString());
                        insertSetting.Parameters.AddWithValue("@ID", guildId.ToString());

                        await insertSetting.ExecuteNonQueryAsync();

                    }

                }

                minimalDatabase.Close();

            }

            _minimalSettings.AddOrUpdate(guildId, minimal, (id, oldminimal) => oldminimal = minimal);

        }

        public async Task SetPrefix(ulong guildId, string prefix)
        {

            var guildIdString = guildId.ToString();

            using (var prefixesDatabase = new SqliteConnection(DatabasePath))
            {

                bool doesPrefixExist;

                await prefixesDatabase.OpenAsync();

                using (SqliteCommand checkExist = prefixesDatabase.CreateCommand())
                {

                    checkExist.CommandText = $"select * from {PrefixTable} where id like @ID";
                    checkExist.Parameters.Add("@ID", SqliteType.Text);
                    checkExist.Parameters["@ID"].Value = guildIdString;

                    doesPrefixExist = (await checkExist.ExecuteScalarAsync()) == null ? false : true;

                }

                if (doesPrefixExist)
                {

                    using (SqliteCommand updatePrefix = prefixesDatabase.CreateCommand())
                    {

                        updatePrefix.CommandText = $"update {PrefixTable} set prefix = @PREFIX where id = @ID";
                        updatePrefix.Parameters.Add("@PREFIX", SqliteType.Text);
                        updatePrefix.Parameters.Add("@ID", SqliteType.Text);
                        updatePrefix.Parameters["@PREFIX"].Value = prefix;
                        updatePrefix.Parameters["@ID"].Value = guildIdString;

                        await updatePrefix.ExecuteNonQueryAsync();

                    }

                }
                else
                {

                    using (SqliteCommand insertPrefix = prefixesDatabase.CreateCommand())
                    {

                        insertPrefix.CommandText = $"insert into {PrefixTable} (id, prefix) values (@ID, @PREFIX)";
                        insertPrefix.Parameters.Add("@ID", SqliteType.Text);
                        insertPrefix.Parameters.Add("@PREFIX", SqliteType.Text);
                        insertPrefix.Parameters["@ID"].Value = guildIdString;
                        insertPrefix.Parameters["@PREFIX"].Value = prefix;

                        await insertPrefix.ExecuteNonQueryAsync();

                    }

                }

                prefixesDatabase.Close();

            }

            _guildPrefixes.AddOrUpdate(guildId, prefix, (id, oldprefix) => oldprefix = prefix);

        }

    }
}
