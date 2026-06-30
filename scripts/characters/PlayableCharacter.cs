using Godot;

public partial class PlayableCharacter : CharacterBody2D, IMoveable
{
	[Signal] public delegate void OpenInventoryEventHandler();
	[Signal] public delegate void OpenMenuEventHandler(MenuType menuType);
	[Signal] public delegate void PlayDeathAnimationEventHandler(string animationName);

	// Animation properties
	[Export] private AnimatedSprite2D _animatedSprite;
	
	private string _animWalkUp = GlobalVariables.DefaultAnimWalkUp;
	private string _animWalkDown = GlobalVariables.DefaultAnimWalkDown;
	private string _animWalkLeft = GlobalVariables.DefaultAnimWalkLeft;
	private string _animWalkRight = GlobalVariables.DefaultAnimWalkRight;

	private string _animRunUp = GlobalVariables.DefaultAnimRunUp;
	private string _animRunDown = GlobalVariables.DefaultAnimRunDown;
	private string _animRunLeft = GlobalVariables.DefaultAnimRunLeft;
	private string _animRunRight = GlobalVariables.DefaultAnimRunRight;

	private string _animIdleUp = GlobalVariables.DefaultAnimIdleUp;
	private string _animIdleDown = GlobalVariables.DefaultAnimIdleDown;
	private string _animIdleLeft = GlobalVariables.DefaultAnimIdleLeft;
	private string _animIdleRight = GlobalVariables.DefaultAnimIdleRight;

	// Status properties
	private EntityState _currentState = EntityState.ACTIVE;

	// Movement properties
	private float _speed = GlobalVariables.DefaultSpeed;
	private float _runSpeed = GlobalVariables.DefaultRunSpeed;
	private MovementAxis _lastAxis = MovementAxis.NONE;

	// Interaction properties
	private Item _currentItem = null;
	private float _interactionRange = GlobalVariables.DefaultInteractionRange;
	private FacingDirection _currentFacing = GlobalVariables.DefaultFacingDirection;
	
	public override void _Ready()
	{
		CollisionMask = GlobalVariables.SolidMask;
		PlayDeathAnimation += OnPlayDeathAnimation;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (_currentState != EntityState.ACTIVE) return;

		HandleMovement();
		HandleInteraction();
		HandleShortcuts();
	}

	// === Getters ===
	public PlayableCharacterConfig getCurrentConfig()
	{
		return new PlayableCharacterConfig
		{
			PosX = Position.X,
			PosY = Position.Y,
			Speed = _speed,
			RunSpeed = _runSpeed,
			InteractionRange = _interactionRange,
			FacingDirection = _currentFacing,
 			AnimWalkUp = _animWalkUp,
    		AnimWalkDown = _animWalkDown,
    		AnimWalkLeft = _animWalkLeft,
    		AnimWalkRight = _animWalkRight,
    		AnimRunUp = _animRunUp,
    		AnimRunDown = _animRunDown,
    		AnimRunLeft = _animRunLeft,
    		AnimRunRight = _animRunRight,
    		AnimIdleUp = _animIdleUp,
    		AnimIdleDown = _animIdleDown,
    		AnimIdleLeft = _animIdleLeft,
    		AnimIdleRight = _animIdleRight
		};
	}

	// === Setter ===
	public void SetCurrentItem(Item item)
	{
		_currentItem = item;
	}
	public void SetState(EntityState state)
	{
		_currentState = state;
		Visible = state != EntityState.HIDDEN;
		if (state != EntityState.ACTIVE)
		{
			Velocity = Vector2.Zero;
		}
	}
	public void ApplyConfig(PlayableCharacterConfig config)
	{
		Position = new Vector2(config.PosX, config.PosY);
		
		_speed = config.Speed;
		_runSpeed = config.RunSpeed;

		_interactionRange = config.InteractionRange;

		_currentFacing = config.FacingDirection;
		
		_animWalkUp = config.AnimWalkUp;
		_animWalkDown = config.AnimWalkDown;
		_animWalkLeft = config.AnimWalkLeft;
		_animWalkRight = config.AnimWalkRight;

		_animRunUp = config.AnimRunUp;
		_animRunDown = config.AnimRunDown;
		_animRunLeft = config.AnimRunLeft;
		_animRunRight = config.AnimRunRight;

		_animIdleUp = config.AnimIdleUp;
		_animIdleDown = config.AnimIdleDown;
		_animIdleLeft = config.AnimIdleLeft;
		_animIdleRight = config.AnimIdleRight;
	}

	// === Movement ===
	private void HandleMovement()
	{
		Vector2 movementInput = GetMovementInput();

		if (movementInput != Vector2.Zero)
		{
			_currentFacing = FacingDirectionFromVector(movementInput);

			bool isRunning = Input.IsActionPressed(GlobalVariables.RunAction);
			if (isRunning)
			{
				Run(movementInput);
			}
			else
			{
				Move(movementInput);
			}
		}
		else
		{
			Stop();
		}
	}
	private Vector2 GetMovementInput()
	{
		float horizontal = Input.GetAxis(GlobalVariables.MoveLeftAction, GlobalVariables.MoveRightAction);
		float vertical = Input.GetAxis(GlobalVariables.MoveUpAction, GlobalVariables.MoveDownAction);

		UpdateLastAxis();

		if (horizontal != 0 && vertical != 0)
		{
			return _lastAxis == MovementAxis.HORIZONTAL
				? new Vector2(horizontal, 0)
				: new Vector2(0, vertical);
		}

		return new Vector2(horizontal, vertical);
	}
	private void UpdateLastAxis()
	{
		bool leftOrRightJustPressed = Input.IsActionJustPressed(GlobalVariables.MoveLeftAction)
			|| Input.IsActionJustPressed(GlobalVariables.MoveRightAction);
		bool upOrDownJustPressed = Input.IsActionJustPressed(GlobalVariables.MoveUpAction)
			|| Input.IsActionJustPressed(GlobalVariables.MoveDownAction);

		if (leftOrRightJustPressed) _lastAxis = MovementAxis.HORIZONTAL;
		if (upOrDownJustPressed) _lastAxis = MovementAxis.VERTICAL;
	}
	public void Move(Vector2 direction)
	{
		Velocity = direction * _speed;
		MoveWithCollisionClamp(Velocity);
		PlayWalkAnimation();
	}
	private void Run(Vector2 direction)
	{
		Velocity = direction * _runSpeed;
		MoveWithCollisionClamp(Velocity);
		PlayRunAnimation();
	}
	private void MoveWithCollisionClamp(Vector2 velocity)
	{
		Vector2 motion = velocity * (float)GetPhysicsProcessDeltaTime();
		var collision = MoveAndCollide(motion, testOnly: true);

		if (collision != null)
			Position += collision.GetTravel();
		else
			Position += motion;
	}
	public void Stop()
	{
		Velocity = Vector2.Zero;
		MoveAndSlide();
		PlayIdleAnimation();
	}
	private FacingDirection FacingDirectionFromVector(Vector2 direction)
	{
		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
		{
			return direction.X > 0 ? FacingDirection.RIGHT : FacingDirection.LEFT;
		}
		else
		{
			return direction.Y > 0 ? FacingDirection.DOWN : FacingDirection.UP;
		}
	}
	private void PlayWalkAnimation()
	{
		string animName = _currentFacing switch
		{
			FacingDirection.UP => _animWalkUp,
			FacingDirection.DOWN => _animWalkDown,
			FacingDirection.LEFT => _animWalkLeft,
			FacingDirection.RIGHT => _animWalkRight,
			_ => _animWalkDown
		};
		_animatedSprite.Play(animName);
	}
	private void PlayRunAnimation()
	{
		string animName = _currentFacing switch
		{
			FacingDirection.UP => _animRunUp,
			FacingDirection.DOWN => _animRunDown,
			FacingDirection.LEFT => _animRunLeft,
			FacingDirection.RIGHT => _animRunRight,
			_ => _animRunDown
		};
		_animatedSprite.Play(animName);
	}
	private void PlayIdleAnimation()
	{
		string animName = _currentFacing switch
		{
			FacingDirection.UP => _animIdleUp,
			FacingDirection.DOWN => _animIdleDown,
			FacingDirection.LEFT => _animIdleLeft,
			FacingDirection.RIGHT => _animIdleRight,
			_ => _animIdleDown
		};
		_animatedSprite.Play(animName);
	}
	
	// === Interaction ===
	private Vector2 GetCurrentSpriteSize()
	{
		if (_animatedSprite?.SpriteFrames == null)
		{
			return Vector2.Zero;
		}

		string anim = _animatedSprite.Animation;
		int frame = _animatedSprite.Frame;
		Texture2D tex = _animatedSprite.SpriteFrames.GetFrameTexture(anim, frame);

		if (tex == null)
		{
			return Vector2.Zero;
		}

		return tex.GetSize() * _animatedSprite.Scale.Abs();
	}
	// Builds the interaction rectangle from facing, drawn from the sprite center (GlobalPosition):
	// - UP/DOWN: width = sprite width, length = InteractionRange, drawn up/down.
	// - LEFT/RIGHT: height = sprite height, length = InteractionRange, drawn left/right.
	// The rectangle sits flush against the sprite edge (never overlaps the player body).
	private (Vector2 center, Vector2 size) BuildInteractionRect()
	{
		Vector2 spriteSize = GetCurrentSpriteSize();
		float spriteWidth = spriteSize.X;
		float spriteHeight = spriteSize.Y;

		Vector2 size;
		Vector2 dir;
		float halfSpriteAlongFacing;

		switch (_currentFacing)
		{
			case FacingDirection.UP:
				size = new Vector2(spriteWidth, _interactionRange);
				dir = Vector2.Up;
				halfSpriteAlongFacing = spriteHeight * 0.5f;
				break;
			case FacingDirection.DOWN:
				size = new Vector2(spriteWidth, _interactionRange);
				dir = Vector2.Down;
				halfSpriteAlongFacing = spriteHeight * 0.5f;
				break;
			case FacingDirection.LEFT:
				size = new Vector2(_interactionRange, spriteHeight);
				dir = Vector2.Left;
				halfSpriteAlongFacing = spriteWidth * 0.5f;
				break;
			case FacingDirection.RIGHT:
			default:
				size = new Vector2(_interactionRange, spriteHeight);
				dir = Vector2.Right;
				halfSpriteAlongFacing = spriteWidth * 0.5f;
				break;
		}

		float pushDistance = halfSpriteAlongFacing + _interactionRange * 0.5f;
		Vector2 center = GlobalPosition + dir * pushDistance;

		return (center, size);
	}
	private IInteractable FindInteractableInFront()
	{
		var (center, size) = BuildInteractionRect();

		var rectShape = new RectangleShape2D { Size = size };

		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = rectShape,
			Transform = new Transform2D(0, center),
			CollisionMask = GlobalVariables.InteractionMask,
			CollideWithBodies = true,
			CollideWithAreas = true
		};

		PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;
		var results = space.IntersectShape(query, maxResults: 8);

		IInteractable nearest = null;
		float nearestDistanceSq = float.MaxValue;

		foreach (var hit in results)
		{
			if (hit["collider"].AsGodotObject() is IInteractable interactable
				&& interactable is Node2D node2D)
			{
				float distanceSq = GlobalPosition.DistanceSquaredTo(node2D.GlobalPosition);
				if (distanceSq < nearestDistanceSq)
				{
					nearestDistanceSq = distanceSq;
					nearest = interactable;
				}
			}
		}

		return nearest;
	}
	private void HandleInteraction()
	{
		if (!Input.IsActionJustPressed(GlobalVariables.AcceptAction))
		{
			return;
		}

		IInteractable target = FindInteractableInFront();
		if (target == null)
		{
			return;
		}

		if (_currentItem != null)
		{
			UseItemOn(_currentItem, target);
			_currentItem = null;
		}
		else
		{
			Examine(target);
		}
	}
	private void Examine(IInteractable target)
	{
		target.Interact();
	}
	private void UseItemOn(Item item, IInteractable target)
	{
		target.InteractWithItem(item);
	}

	// === Shortcut ===
	private void HandleShortcuts()
	{
		if(Input.IsActionJustPressed(GlobalVariables.OpenInventoryAction))
		{
			EmitSignal(SignalName.OpenInventory);
			return;
		}

		if(Input.IsActionJustPressed(GlobalVariables.OpenPauseMenu))
		{
			EmitSignal(SignalName.OpenMenu, Variant.From(MenuType.PAUSE));
			return;
		}
	}

	// === Death ===
	private async void OnPlayDeathAnimation(string animationName)
	{
		SetState(EntityState.FROZEN);
		_animatedSprite.Play(animationName);

		await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

		EmitSignal(SignalName.OpenMenu, Variant.From(MenuType.GAME_OVER));
	}
}