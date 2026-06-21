using Godot;

public partial class PlayableCharacter : CharacterBody2D, IMoveable, IDamageable
{
	// Action constants
	private const string MoveUpAction = "move_up";
	private const string MoveDownAction = "move_down";
	private const string MoveLeftAction = "move_left";
	private const string MoveRightAction = "move_right";
	private const string RunAction = "run";
	private const string InteractAction = "interact";

	// Movement properties
	[Export] public float Speed { get; private set; } = 150.0f;
	[Export] public float RunSpeed { get; private set; } = 250.0f;
	[Export] public FacingDirection InitialFacing { get; private set; } = FacingDirection.DOWN;

	// Status properties
	[Export] public int MaxHealth { get; private set; } = 3;
	[Export] public EntityState InitialState { get; private set; } = EntityState.ACTIVE;

	// Interaction properties
	[Export] public float InteractionRange { get; private set; } = 16.0f;
	// Physics layer the IInteractables live on, used for interaction raycasting
	[Export(PropertyHint.Layers2DPhysics)] public uint InteractionMask { get; private set; } = 1;

	// Animation properties
	[Export] public string AnimWalkUp { get; private set; } = "mc_walk_up";
	[Export] public string AnimWalkDown { get; private set; } = "mc_walk_down";
	[Export] public string AnimWalkLeft { get; private set; } = "mc_walk_left";
	[Export] public string AnimWalkRight { get; private set; } = "mc_walk_right";
	[Export] public string AnimRunUp { get; private set; } = "mc_run_up";
	[Export] public string AnimRunDown { get; private set; } = "mc_run_down";
	[Export] public string AnimRunLeft { get; private set; } = "mc_run_left";
	[Export] public string AnimRunRight { get; private set; } = "mc_run_right";
	[Export] public string AnimIdleUp { get; private set; } = "mc_idle_up";
	[Export] public string AnimIdleDown { get; private set; } = "mc_idle_down";
	[Export] public string AnimIdleLeft { get; private set; } = "mc_idle_left";
	[Export] public string AnimIdleRight { get; private set; } = "mc_idle_right";


	// Private fields
	private AnimatedSprite2D _animatedSprite;
	private FacingDirection _currentFacing;
	private Item _currentItem;
	private int _currentHealth;
	private EntityState _currentState;
	private bool _isDead = false;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_currentFacing = InitialFacing;
		SetState(InitialState);
		PlayIdleAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_currentState != EntityState.ACTIVE) return;

		HandleMovementInput();
		HandleInteractionInput();
	}


	// === Getters ===
	public bool IsDead => _isDead;
	public FacingDirection Facing => _currentFacing;
	public EntityState State => _currentState;


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


	// === Movement ===
	private void HandleMovementInput()
	{
		Vector2 inputDirection = Input.GetVector(
			MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction
		);

		if (inputDirection != Vector2.Zero)
		{
			_currentFacing = FacingDirectionFromVector(inputDirection);

			bool isRunning = Input.IsActionPressed(RunAction);
			if (isRunning)
			{
				Run(inputDirection);
			}
			else
			{
				Move(inputDirection);
			}
		}
		else
		{
			Stop();
		}
	}

	public void Move(Vector2 direction)
	{
		Velocity = direction * Speed;
		MoveAndSlide();
		PlayWalkAnimation();
	}

	private void Run(Vector2 direction)
	{
		Velocity = direction * RunSpeed;
		MoveAndSlide();
		PlayRunAnimation();
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
			FacingDirection.UP => AnimWalkUp,
			FacingDirection.DOWN => AnimWalkDown,
			FacingDirection.LEFT => AnimWalkLeft,
			FacingDirection.RIGHT => AnimWalkRight,
			_ => AnimWalkDown
		};
		_animatedSprite.Play(animName);
	}

	private void PlayRunAnimation()
	{
		string animName = _currentFacing switch
		{
			FacingDirection.UP => AnimRunUp,
			FacingDirection.DOWN => AnimRunDown,
			FacingDirection.LEFT => AnimRunLeft,
			FacingDirection.RIGHT => AnimRunRight,
			_ => AnimRunDown
		};
		_animatedSprite.Play(animName);
	}

	private void PlayIdleAnimation()
	{
		string animName = _currentFacing switch
		{
			FacingDirection.UP => AnimIdleUp,
			FacingDirection.DOWN => AnimIdleDown,
			FacingDirection.LEFT => AnimIdleLeft,
			FacingDirection.RIGHT => AnimIdleRight,
			_ => AnimIdleDown
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
				size = new Vector2(spriteWidth, InteractionRange);
				dir = Vector2.Up;
				halfSpriteAlongFacing = spriteHeight * 0.5f;
				break;
			case FacingDirection.DOWN:
				size = new Vector2(spriteWidth, InteractionRange);
				dir = Vector2.Down;
				halfSpriteAlongFacing = spriteHeight * 0.5f;
				break;
			case FacingDirection.LEFT:
				size = new Vector2(InteractionRange, spriteHeight);
				dir = Vector2.Left;
				halfSpriteAlongFacing = spriteWidth * 0.5f;
				break;
			case FacingDirection.RIGHT:
			default:
				size = new Vector2(InteractionRange, spriteHeight);
				dir = Vector2.Right;
				halfSpriteAlongFacing = spriteWidth * 0.5f;
				break;
		}

		float pushDistance = halfSpriteAlongFacing + InteractionRange * 0.5f;
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
			CollisionMask = InteractionMask,
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

	private void HandleInteractionInput()
	{
		if (!Input.IsActionJustPressed(InteractAction))
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

		//GetTree().CallGroup("GameManager", "OnPlayerDied", source);
	}
}