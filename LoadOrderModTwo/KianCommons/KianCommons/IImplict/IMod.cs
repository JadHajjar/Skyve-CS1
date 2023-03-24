namespace KianCommons.IImplict {
    using ICities;

    internal interface IMod {
        void OnEnabled();
        void OnDisabled();
    }

    internal interface IModWithSettings : IUserMod {
        void OnSettingsUI(UIHelper helper);
    }

}
