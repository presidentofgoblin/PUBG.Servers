namespace PlayerUnknown.Logic.Interfaces.Players
{
    public interface IProfile : IObject
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        string Nickname
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the profile status.
        /// </summary>
        string ProfileStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the invite allow.
        /// </summary>
        string InviteAllow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the skin.
        /// </summary>
        ISkin Skin
        {
            get;
            set;
        }
    }
}
