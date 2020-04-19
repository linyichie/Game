using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginWin : Window {
    [SerializeField] private Button m_LanguageCN;
    [SerializeField] private Button m_LanguageEN;
    [SerializeField] private Button m_LanguageJP;
    [SerializeField] private Button m_LanguageKR;
    [SerializeField] private Text m_LoginLabel;

    protected override void OnInitialize() {
        m_LanguageCN.onClick.AddListener(() => { Localization.Set("CN"); });
        m_LanguageEN.onClick.AddListener(() => { Localization.Set("EN"); });
        m_LanguageJP.onClick.AddListener(() => { Localization.Set("JP"); });
        m_LanguageKR.onClick.AddListener(() => { Localization.Set("KR"); });

        Localization.languageChange += () => {
            ConfigLoader.ReloadLocalization();
            ConfigLoader.compelted += () => { DisplayLabel(); };
        };
    }

    protected override void OnReadyOepn() {
        DisplayLabel();
    }

    private void DisplayLabel() {
        m_LoginLabel.text = Language.Get("Login");
    }
}