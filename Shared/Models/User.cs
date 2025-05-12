﻿using LiteDB;

namespace Shared.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
