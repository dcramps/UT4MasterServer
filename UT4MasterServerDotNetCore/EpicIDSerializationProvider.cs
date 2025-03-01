﻿using MongoDB.Bson.Serialization;

namespace UT4MasterServer;

public class EpicIDSerializationProvider : IBsonSerializationProvider
{
	public IBsonSerializer GetSerializer(Type type)
	{
		if (type == typeof(EpicID))
			return new EpicIDSerializer();

		// returning null here seems to be fine.
		// it probably signals to the caller that we don't have serializer for specified type.
		return null!;
	}
}