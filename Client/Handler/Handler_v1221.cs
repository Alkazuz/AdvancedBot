using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Handler
{
    public class Handler_v1221 : Handler_v18
    {
        public Handler_v1221(MinecraftClient mc) : base(mc) { }
        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_12_1; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            PacketTypeS type = (PacketTypeS)pkt.ID;
            //if (type != PacketTypeS.rel_entity_move &&
            //    type != PacketTypeS.entity_move_look &&
            //    type != PacketTypeS.entity_teleport && 
            //    type != PacketTypeS.entity_look &&
            //    type != PacketTypeS.entity_head_rotation &&
            //    type != PacketTypeS.entity_velocity) 
            //{
            //    Debug.WriteLine($"{DateTime.Now:HH:mm:ss:fff} (0x{pkt.ID:X2} {type})");
            //}

            switch (type) {
                case PacketTypeS.keep_alive:            pkt.ID = 0x00; base.HandlePacket(pkt); break; //Keep-alive
                case PacketTypeS.login:                 pkt.ID = 0x01; base.HandlePacket(pkt); break; //Join game
                case PacketTypeS.chat:                  pkt.ID = 0x02; base.HandlePacket(pkt); break; //Chat message
                case PacketTypeS.update_health:         pkt.ID = 0x06; base.HandlePacket(pkt); break; //Update health
                case PacketTypeS.respawn:               pkt.ID = 0x07; base.HandlePacket(pkt); break; //Respawn
                case PacketTypeS.held_item_slot:        Client.SetHotbarSlot(pkt.ReadByte()); break;  //Held item change
                case PacketTypeS.entity_velocity:       pkt.ID = 0x12; base.HandlePacket(pkt); break; //Entity velocity
                case PacketTypeS.entity_destroy:        pkt.ID = 0x13; base.HandlePacket(pkt); break; //Destroy entities
                case PacketTypeS.entity_effect:         pkt.ID = 0x1D; base.HandlePacket(pkt); break; //Effect
                case PacketTypeS.remove_entity_effect:  pkt.ID = 0x1E; base.HandlePacket(pkt); break; //Remove effect
                case PacketTypeS.multi_block_change:    pkt.ID = 0x22; base.HandlePacket(pkt); break; //Multi block change
                case PacketTypeS.block_change:          pkt.ID = 0x23; base.HandlePacket(pkt); break; //Block change
                case PacketTypeS.explosion:             pkt.ID = 0x27; base.HandlePacket(pkt); break; //Explosion
                case PacketTypeS.game_state_change:     pkt.ID = 0x2B; base.HandlePacket(pkt); break; //Change game state
                case PacketTypeS.open_window:           pkt.ID = 0x2D; base.HandlePacket(pkt); break; //Open window
                case PacketTypeS.close_window:          pkt.ID = 0x2E; base.HandlePacket(pkt); break; //Close window
                case PacketTypeS.set_slot:              pkt.ID = 0x2F; base.HandlePacket(pkt); break; //Set slot
                case PacketTypeS.window_items:          pkt.ID = 0x30; base.HandlePacket(pkt); break; //Window items
                case PacketTypeS.transaction:           pkt.ID = 0x32; base.HandlePacket(pkt); break; //Confirm transaction

                // Removed Update Sign (clientbound) (0x46); Update Block Entity with action 9 should be used instead

                case PacketTypeS.player_info: pkt.ID = 0x38; base.HandlePacket(pkt); break; //Player list
                case PacketTypeS.title: pkt.ID = 0x45; base.HandlePacket(pkt); break; //Title
                ///////////Changed packets////////////
                case PacketTypeS.named_entity_spawn: { //spawn player
                    int entityID = pkt.ReadVarInt();
                    UUID uuid = new UUID(pkt.ReadLong(), pkt.ReadLong());
                    double x = pkt.ReadDouble();
                    double y = pkt.ReadDouble();
                    double z = pkt.ReadDouble();
                    float yaw = (float)(pkt.ReadSByte() * 360) / 256f;
                    float pitch = (float)(pkt.ReadSByte() * 360) / 256f;

                    MPPlayer player = new MPPlayer(entityID, uuid);
                    player.SetPos(x, y, z);
                    player.SetRotation(yaw, pitch);

                    Client.PlayerManager.Players[entityID] = player;
                    break;
                }
                case PacketTypeS.rel_entity_move: { //entity relative move
                    int entityId = pkt.ReadVarInt();
                    double dx = pkt.ReadShort() / 4096.0;
                    double dy = pkt.ReadShort() / 4096.0;
                    double dz = pkt.ReadShort() / 4096.0;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                        player.X += dx;
                        player.Y += dy;
                        player.Z += dz;
                    }
                    break;
                }
                case PacketTypeS.entity_look: { //entity look
                    int entityId = pkt.ReadVarInt();
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                        player.SetRotation(yaw, pitch);
                    break;
                }
                case PacketTypeS.entity_move_look: {// entity look and relative move
                    int entityId = pkt.ReadVarInt();

                    double dx = pkt.ReadShort() / 4096.0;
                    double dy = pkt.ReadShort() / 4096.0;
                    double dz = pkt.ReadShort() / 4096.0;

                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                        player.X += dx;
                        player.Y += dy;
                        player.Z += dz;
                        player.SetRotation(yaw, pitch);
                    }
                    break;
                }
                case PacketTypeS.entity_teleport: { //entity teleport
                    int entityId = pkt.ReadVarInt();

                    double x = pkt.ReadDouble();
                    double y = pkt.ReadDouble();
                    double z = pkt.ReadDouble();
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                        player.SetPos(x, y, z);
                        player.SetRotation(yaw, pitch);
                    }
                    break;
                }
                case PacketTypeS.entity_head_rotation: { //entity head look
                    int entityId = pkt.ReadVarInt();
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                        player.Yaw = yaw;
                    break;
                }
                case PacketTypeS.map_chunk: if (Client.MapAndPhysics) HandleChunkData(pkt); break;
                case PacketTypeS.unload_chunk: {
                    if (!Client.MapAndPhysics) break;

                    int chunkX = pkt.ReadInt();
                    int chunkZ = pkt.ReadInt();
                    World.SetChunk(chunkX, chunkZ, null);
                    break;
                }
                case PacketTypeS.position: { //Position
                    double x = pkt.ReadDouble();
                    double y = pkt.ReadDouble();
                    double z = pkt.ReadDouble();
                    float yaw = pkt.ReadFloat();
                    float pitch = pkt.ReadFloat();
                    byte flags = pkt.ReadByte();
                    int teleportId = pkt.ReadVarInt();

                    const int FLAG_X = 0x01;
                    const int FLAG_Y = 0x02;
                    const int FLAG_Z = 0x04;
                    const int FLAG_Y_ROT = 0x08;
                    const int FLAG_X_ROT = 0x10;

                    if ((flags & FLAG_X) != 0) x += Player.PosX;
                    else Player.MotionX = 0.0;

                    if ((flags & FLAG_Y) != 0) y += Player.AABB.MinY;
                    else Player.MotionY = 0.0;

                    if ((flags & FLAG_Z) != 0) z += Player.PosZ;
                    else Player.MotionZ = 0.0;

                    if ((flags & FLAG_X_ROT) != 0) pitch += Player.Pitch;
                    if ((flags & FLAG_Y_ROT) != 0) yaw += Player.Yaw;

                    Player.SetPosition(x, y, z);
                    Player.Yaw = yaw % 360.0f;
                    Player.Pitch = pitch % 360.0f;

                    Client.SendPacket(new PacketTeleportConfirm(teleportId));
                    break;
                }
                case PacketTypeS.kick_disconnect: //disconnect
                    string reason = ChatParser.ParseJson(pkt.ReadString());
                    Client.HandlePacketDisconnect(reason);
                    return false;
            }
            return true;
        }

        protected void HandleChunkData(ReadBuffer pkt)
        {
            int x = pkt.ReadInt();
            int z = pkt.ReadInt();
            bool full = pkt.ReadBoolean();
            int mask = pkt.ReadVarInt();

            if (Client.LimitChunks) {
                int dx = (x * 16 + 8) - Utils.Floor(Player.PosX);
                int dz = (z * 16 + 8) - Utils.Floor(Player.PosZ);
                if (dx * dx + dz * dz > MinecraftClient.CHUNK_LIMIT)
                    return;
            }

            Chunk chunk = full ? new Chunk(x, z) : World.GetChunk(x, z);
            if (chunk == null) {
                chunk = new Chunk(x, z);
            }

            int dataLen = pkt.ReadVarInt();
            DecodeChunk(chunk, full, Client.Dimension == 0, mask, pkt);
            //ReadChunkColumn(chunk, full, mask, new ReadBuffer(pkt.ReadByteArray(pkt.ReadVarInt()), false));
            World.SetChunk(x, z, chunk);

            /* int blockEntityCount = pkt.ReadVarInt();
               for (int i = 0; i < blockEntityCount; i++) {
                   CompoundTag tag = pkt.ReadNBT();
               }*/
        }
        public static void DecodeChunk(Chunk chunk, bool full, bool overworld, int mask, ReadBuffer pkt)
        {
            for (int y = 0; y < 16; y++) {
                if ((mask & 1 << y) != 0) {
                    ChunkSection section = new ChunkSection();

                    int bits = pkt.ReadByte();
                    bool usePalette = bits <= 8;

                    int paletteLen = pkt.ReadVarInt();
                    int[] palette = new int[paletteLen];
                    for (int i = 0; i < paletteLen; i++)
                        palette[i] = pkt.ReadVarInt();

                    ulong maxEntryValue = (1ul << bits) - 1ul;
                    ulong[] data = new ulong[pkt.ReadVarInt()];
                    for (int i = 0, l = data.Length; i < l; i++)
                        data[i] = (ulong)pkt.ReadLong();

                    for (int i = 0; i < 4096; i++) {
                        int bitIndex = i * bits;
                        int startLong = bitIndex / 64;
                        int startOffset = bitIndex % 64;
                        int endLong = ((i + 1) * bits - 1) / 64;

                        int block;
                        if (startLong == endLong)
                            block = (int)(data[startLong] >> startOffset & maxEntryValue);
                        else {
                            int endOffset = 64 - startOffset;
                            block = (int)((data[startLong] >> startOffset | data[endLong] << endOffset) & maxEntryValue);
                        }
                        if (usePalette) {
                            block = palette[block];
                        }

                        int meta = block & 0xF;
                        section.Metadata[i / 2] |= (byte)(i % 2 == 0 ? meta : (meta << 4));
                        section.Blocks[i] = (byte)(block >> 4);
                    }

                    chunk.Sections[y] = section;
                    pkt.Skip(2048); //pkt.ReadByteArray(2048); //blocklight

                    if (overworld) {
                        pkt.Skip(2048); //pkt.ReadByteArray(2048); //skylight
                    }
                } else if (full && chunk.Sections[y] != null) {
                    chunk.Sections[y] = null;
                }
            }
        }

        public override bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf)
        {
           // Debug.WriteLine($"{DateTime.Now:HH:mm:ss:fff} (0x{v18id:X2} {pkt.GetType().Name})");
            switch (v18id) {
                case 0x00: v18id = (int)PacketTypeC.keep_alive;         return false; //Keep-alive
                case 0x01: v18id = (int)PacketTypeC.chat;               return false; //Chat message
                case 0x03: v18id = (int)PacketTypeC.flying;             return false; //Player update
                case 0x04: v18id = (int)PacketTypeC.position;           return false; //Player pos
                case 0x05: v18id = (int)PacketTypeC.look;               return false; //Player look
                case 0x06: v18id = (int)PacketTypeC.position_look;      return false; //Player pos and look
                case 0x09: v18id = (int)PacketTypeC.held_item_slot;     return false; //Held item change
                case 0x0B: v18id = (int)PacketTypeC.entity_action;      return false; //Entity action - “Open inventory” is now sent via the Client Status packet.
                case 0x0D: v18id = (int)PacketTypeC.close_window;       return false; //Close window
                case 0x0E: v18id = (int)PacketTypeC.window_click;       return false; //Click window
                case 0x0F: v18id = (int)PacketTypeC.transaction;        return false; //Confirm transaction
                case 0x10: v18id = (int)PacketTypeC.set_creative_slot;  return false; //Creative inv action
                case 0x17: v18id = (int)PacketTypeC.custom_payload;     return false; //Plugin msg
                case 0x02: {
                    PacketUseEntity p = (PacketUseEntity)pkt;
                    buf.WriteVarInt((int)PacketTypeC.use_entity);
                    buf.WriteVarInt(p.EntityID);
                    buf.WriteVarInt(p.MouseButton);
                    if (p.MouseButton == 2) {
                        /*buf.WriteFloat(p.TargetX);
                        buf.WriteFloat(p.TargetY);
                        buf.WriteFloat(p.TargetZ);*/
                    }
                    if (p.MouseButton == 0 || p.MouseButton == 2) {
                        buf.WriteVarInt(0);//hand
                    }
                    return true;
                }
                case 0x07: {
                    PacketPlayerDigging p = (PacketPlayerDigging)pkt;
                    buf.WriteVarInt((int)PacketTypeC.block_dig);
                    buf.WriteVarInt((int)p.Status);
                    buf.WriteLocation(new Vec3i(p.X, p.Y, p.Z));
                    buf.WriteByte(p.Face);
                    return true;
                }
                case 0x16: {
                    PacketClientStatus p = (PacketClientStatus)pkt;
                    buf.WriteVarInt((int)PacketTypeC.client_command);
                    buf.WriteVarInt(p.ActionID);
                    return true;
                }
                case 0x15: {
                    PacketClientSettings p = (PacketClientSettings)pkt;
                    buf.WriteVarInt((int)PacketTypeC.settings);
                    buf.WriteString(p.Locate);
                    buf.WriteByte(p.ViewDistance);
                    buf.WriteVarInt(p.ChatFlags);
                    buf.WriteBoolean(p.ChatColors);
                    buf.WriteByte(0x7F);//skin parts, all flags
                    buf.WriteVarInt(0);//hand
                    return true;
                }
                case 0x0A: { //Animation->SwingArm
                    buf.WriteVarInt((int)PacketTypeC.arm_animation);
                    buf.WriteVarInt(0);//hand
                    return true;
                }
                case 0x08: {
                    PacketBlockPlace p = (PacketBlockPlace)pkt;
                    if(p.Direction == 255) {
                        /* Indicates that the currently held item should have its state updated 
                         * such as eating food, pulling back bows, using buckets, etc. 
                         * Location is always set to 0/0/0, Face is always set to -Y. 
                         */
                        buf.WriteVarInt((int)PacketTypeC.block_dig);
                        buf.WriteVarInt((int)DiggingStatus.FinishUse);
                        buf.WriteLocation(new Vec3i(0, 0, 0));
                        buf.WriteByte(0); //-Y
                        return true;
                    }
                    buf.WriteVarInt((int)PacketTypeC.block_place);
                    buf.WriteLocation(new Vec3i(p.X, p.Y, p.Z));
                    buf.WriteVarInt(p.Direction);
                    buf.WriteVarInt(0);//hand
                    buf.WriteFloat(p.CursorX / 16f);
                    buf.WriteFloat(p.CursorY / 16f);
                    buf.WriteFloat(p.CursorZ / 16f);
                    return true;
                }
                default:
                    Debug.WriteLine($"Unhandled 1.8->1.12.1 packet ({pkt.GetType().Name} 0x{v18id:X2})");
                    return false;
            }
        }

        //[^\r\n]*"(0x[0-9a-f]+)": "(.+)"
        //$2 = $1
        private enum PacketTypeS
        {
            spawn_entity = 0x00,
            spawn_entity_experience_orb = 0x01,
            spawn_entity_weather = 0x02,
            spawn_entity_living = 0x03,
            spawn_entity_painting = 0x04,
            named_entity_spawn = 0x05,
            animation = 0x06,
            statistics = 0x07,
            block_break_animation = 0x08,
            tile_entity_data = 0x09,
            block_action = 0x0a,
            block_change = 0x0b,
            boss_bar = 0x0c,
            difficulty = 0x0d,
            tab_complete = 0x0e,
            chat = 0x0f,
            multi_block_change = 0x10,
            transaction = 0x11,
            close_window = 0x12,
            open_window = 0x13,
            window_items = 0x14,
            craft_progress_bar = 0x15,
            set_slot = 0x16,
            set_cooldown = 0x17,
            custom_payload = 0x18,
            named_sound_effect = 0x19,
            kick_disconnect = 0x1a,
            entity_status = 0x1b,
            explosion = 0x1c,
            unload_chunk = 0x1d,
            game_state_change = 0x1e,
            keep_alive = 0x1f,
            map_chunk = 0x20,
            world_event = 0x21,
            world_particles = 0x22,
            login = 0x23,
            map = 0x24,
            entity = 0x25,
            rel_entity_move = 0x26,
            entity_move_look = 0x27,
            entity_look = 0x28,
            vehicle_move = 0x29,
            open_sign_entity = 0x2a,
            craft_recipe_response = 0x2b,
            abilities = 0x2c,
            combat_event = 0x2d,
            player_info = 0x2e,
            position = 0x2f,
            bed = 0x30,
            unlock_recipes = 0x31,
            entity_destroy = 0x32,
            remove_entity_effect = 0x33,
            resource_pack_send = 0x34,
            respawn = 0x35,
            entity_head_rotation = 0x36,
            select_advancement_tab = 0x37,
            world_border = 0x38,
            camera = 0x39,
            held_item_slot = 0x3a,
            scoreboard_display_objective = 0x3b,
            entity_metadata = 0x3c,
            attach_entity = 0x3d,
            entity_velocity = 0x3e,
            entity_equipment = 0x3f,
            experience = 0x40,
            update_health = 0x41,
            scoreboard_objective = 0x42,
            set_passengers = 0x43,
            teams = 0x44,
            scoreboard_score = 0x45,
            spawn_position = 0x46,
            update_time = 0x47,
            title = 0x48,
            sound_effect = 0x49,
            playerlist_header = 0x4a,
            collect = 0x4b,
            entity_teleport = 0x4c,
            advancements = 0x4d,
            entity_update_attributes = 0x4e,
            entity_effect = 0x4f
        }
        private enum PacketTypeC
        {
            teleport_confirm = 0x00,
            tab_complete = 0x01,
            chat = 0x02,
            client_command = 0x03,
            settings = 0x04,
            transaction = 0x05,
            enchant_item = 0x06,
            window_click = 0x07,
            close_window = 0x08,
            custom_payload = 0x09,
            use_entity = 0x0a,
            keep_alive = 0x0b,
            flying = 0x0c,
            position = 0x0d,
            position_look = 0x0e,
            look = 0x0f,
            vehicle_move = 0x10,
            steer_boat = 0x11,
            craft_recipe_request = 0x12,
            abilities = 0x13,
            block_dig = 0x14,
            entity_action = 0x15,
            steer_vehicle = 0x16,
            crafting_book_data = 0x17,
            resource_pack_receive = 0x18,
            advancement_tab = 0x19,
            held_item_slot = 0x1a,
            set_creative_slot = 0x1b,
            update_sign = 0x1c,
            arm_animation = 0x1d,
            spectate = 0x1e,
            block_place = 0x1f,
            use_item = 0x20
        }
    }
}
