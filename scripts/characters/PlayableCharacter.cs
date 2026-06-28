using Godot;

public partial class PlayableCharacter : CharacterBody2D, IMoveable, IDamageable
{
	[Signal] public delegate void DiedEventHandler(Node2D source);
	[Signal] public delegate void OpenInventoryEventHandler();
	[Signal] public delegate void OpenMenuEventHandler(MenuType menuType);

	// Animation properties
	private AnimatedSprite2D _animatedSprite;
	private FacingDirection _currentFacing;
	
	private string _animWalkUp;
	private string _animWalkDown;
	private string _animWalkLeft;
	private string _animWalkRight;

	private string _animRunUp;
	private string _animRunDown;
	private string _animRunLeft;
	private string _animRunRight;

	private string _animIdleUp;
	private string _animIdleDown;
	private string _animIdleLeft;
	private string _animIdleRight;

	// Status properties
	private int _currentHealth;
	private EntityState _currentState;
	private bool _isDead = false;

	// Movement properties
	private float _speed;
	private float _runSpeed;
	private uint _solidMask;
	private MovementAxis _lastAxis = MovementAxis.NONE;

	// Interaction properties
	private Item _currentItem;
	private float _interactionRange;
	private uint _interactionMask;

	public override void _PhysicsProcess(double delta)
	{
		if (_currentState != EntityState.ACTIVE) return;

		HandleMovement();
		HandleInteraction();
		HandleShortcuts();
	}

	// === Getters ===
	public bool IsDead => _isDead;
	public FacingDirection Facing => _currentFacing;
	public EntityState State => _currentState;
	public PlayableCharacterConfig getCurrentConfig()
	{
		return new PlayableCharacterConfig
		{
			PosX = Position.X,
			PosY = Position.Y,
			FacingDirection = _currentFacing,
			Health = _currentHealth,
			Speed = _speed,
			RunSpeed = _runSpeed,
			InteractionRange = _interactionRange,
			InteractionMask = _interactionMask,
			SolidMask = _solidMask,
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
		if (_isDead && state == EntityState.ACTIVE) return;

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
		CollisionMask = config.SolidMask;

		_currentFacing = config.FacingDirection;
		_currentHealth = config.Health;
		
		_speed = config.Speed;
		_runSpeed = config.RunSpeed;
		
		_interactionRange = config.InteractionRange;
		_interactionMask = config.InteractionMask;
		_solidMask = config.SolidMask;
		
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
			CollisionMask = _interactionMask,
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

	// === Damage ===
	public void TakeDamage(int damage, Node2D source)
	{
		if (_isDead) return;
		if (_currentState != EntityState.ACTIVE) return;

		_currentHealth -= damage;

		if (_currentHealth <= 0)
		{
			Die(source);
		}
	}
	public void Die(Node2D source)
	{
		if (_isDead) return;

		_isDead = true;
		SetState(EntityState.FROZEN);
		EmitSignal(SignalName.Died, source);
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
}