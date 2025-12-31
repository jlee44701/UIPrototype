using RuntimeUI;
using UIEvents;

public class LecternUIPresenter 
{
    LecternViewSO[] m_ViewData;

    LecternUIView m_LecternUiView;
    public LecternUIView LecternUiView {get => m_LecternUiView;set => m_LecternUiView = value;}

    public LecternUIPresenter(LecternViewSO[] viewData) {
        m_ViewData = viewData;
    }
    
    public void OnEnable() => RegisterCallbacks();
    public void OnDisable() => UnregisterCallbacks();
    
    void RegisterCallbacks() {
        DustProphet.SetupComplete += OnSetupCompleted;
        DustProphet.FooterButtonClicked += OnFooterButtonClicked;
        DustProphet.FooterButtonHighlighted += OnButtonHighlighted;
        DustProphet.FooterButtonEntered += OnFooterButtonEntered;
    }
    void UnregisterCallbacks() {
        DustProphet.SetupComplete -= OnSetupCompleted;
        DustProphet.FooterButtonClicked -= OnFooterButtonClicked;
        DustProphet.FooterButtonHighlighted -= OnButtonHighlighted;
        DustProphet.FooterButtonEntered -= OnFooterButtonEntered;
        
    }
    void OnSetupCompleted() => Initialize();
 
    void Initialize() {
        ShowViewByIndex(0);
    }
    void OnFooterButtonClicked(int index) {
        ShowViewByIndex(index);
    }
    void OnFooterButtonEntered(int index) {
           
    }

    void ShowViewByIndex(int index) {
        var view = m_ViewData[index];
        m_LecternUiView.DisplayStatusText(m_ViewData[index].Title);
        //add button click stuff
        OnButtonHighlighted(index);
    }

    void OnButtonHighlighted(int index) {
        LecternUiView.NavigationBar.HighlightButton(index, "lectern__footer-button--selected");
        
        // something 
        // m_DemoSelectionScreen.ShowIcon(m_DemoInfo[index].Icon);
        // m_DemoSelectionScreen.ShowSummary(m_DemoInfo[index].Summary);
        // m_DemoSelectionScreen.ShowTitle(m_DemoInfo[index].Title);
    }
    
    

}
