using Godot;
using Godot.Collections;

public partial class Dialogue : Node2D
{
	[Signal] public delegate void DestroyDialogueEventHandler(Node2D source);
	
	private string _id;
	private Array<DialogueItem> _items;

	private Texture2D _dialogueBoxSprite;
	private Font _titleFont;
	private Font _descriptionFont;
	private int _textSpeed;

	private int _currentPage = 0;
	private int _selectedOption = 0;
	private bool _areStaticComponentsRendered = false;
	private bool _isCurrentItemCompleted = false;

	private CanvasLayer _cgLayer;
	private TextureRect _cgRect;

	public override void _Ready()
	{
		SetupCgLayer();
	}

	public override void _Process(double delta)
	{
		CheckAndUpdateCurrentPage();

		if (!_areStaticComponentsRendered)
			RenderStaticComponents();

		RenderDynamicComponents(delta);

		if(IsCompleted())
		{
			if(_items[_currentPage].Options.Count != 0)
				HandleSelection();
			else
				CheckForDestroyInput();
		}
	}

	private bool IsCompleted() => IsLastPage() && _isCurrentItemCompleted;
	private bool IsLastPage() => _currentPage == _items.Count - 1;


	private void SetupCgLayer()
	{
        _cgLayer = new CanvasLayer
        {
            Layer = 100 // cao hơn các layer khác để CG luôn nằm trên cùng
        };
        AddChild(_cgLayer);

		_cgRect = new TextureRect();
		_cgRect.SetAnchorsPreset(Control.LayoutPreset.FullRect); // phủ toàn màn hình
		_cgRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		_cgRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
		_cgRect.MouseFilter = Control.MouseFilterEnum.Ignore; // không chặn input
		_cgRect.Visible = false;
		_cgLayer.AddChild(_cgRect);
	}
	
	private void CheckAndUpdateCurrentPage()
	{
		if(Input.IsActionJustPressed(GlobalVariables.AcceptAction))
		{
			if (_isCurrentItemCompleted)
			{
				_currentPage++;
				_areStaticComponentsRendered = false;
				_isCurrentItemCompleted = false;
			}
			else
			{
				_isCurrentItemCompleted = true;		
			}
		}
	}
	private void RenderStaticComponents()
	{
		DrawCG();
		// DrawPortrait();
		// DrawDialogueBox();
		// DrawTitle();

		_areStaticComponentsRendered = true;
	}

	private void RenderDynamicComponents(double delta)
	{
		// DrawDescription(delta);	
	}

	private void DrawCG()
	{	
		Texture2D cg = _items[_currentPage].CG;
		if (cg == null)
		{
			_cgRect.Visible = false;
			return;
		}

		if (_cgRect.Texture != cg)
			_cgRect.Texture = cg;

		_cgRect.Visible = true;

	}

	private void HandleSelection()
	{
		// CheckAndUpdateSelectedOption();
		// CheckForAcceptInput();
	}

	private void CheckForDestroyInput()
	{
		if (Input.IsActionJustPressed(GlobalVariables.AcceptAction))
		{
			EmitSignal(SignalName.DestroyDialogue);		
		}
	}
}
