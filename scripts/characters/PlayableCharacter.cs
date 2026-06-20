using Godot;

public partial class PlayableCharacter : CharacterBody2D, IMoveable
{
	// Action constants
	private const string MoveUpAction = "move_up";
	private const string MoveDownAction = "move_down";
	private const string MoveLeftAction = "move_left";
	private const string MoveRightAction = "move_right";
	private const string RunAction = "run";
	private const string InteractAction = "interact";

	// Movement properties
	[Export] public float Speed = 150.0f;
	[Export] public float RunSpeed = 250.0f;
	[Export] public FacingDirection CurrentFacing = FacingDirection.DOWN;

	// Interaction properties
	[Export] public float InteractionRange = 16.0f;
	// Physics layer the IInteractables live on, used for interaction raycasting
	[Export(PropertyHint.Layers2DPhysics)] public uint InteractionMask = 1;

	// Animation properties
	[Export] public string AnimWalkUp = "mc_walk_up";
	[Export] public string AnimWalkDown = "mc_walk_down";
	[Export] public string AnimWalkLeft = "mc_walk_left";
	[Export] public string AnimWalkRight = "mc_walk_right";
	[Export] public string AnimRunUp = "mc_run_up";
	[Export] public string AnimRunDown = "mc_run_down";
	[Export] public string AnimRunLeft = "mc_run_left";
	[Export] public string AnimRunRight = "mc_run_right";
	[Export] public string AnimIdleUp = "mc_idle_up";
	[Export] public string AnimIdleDown = "mc_idle_down";
	[Export] public string AnimIdleLeft = "mc_idle_left";
	[Export] public string AnimIdleRight = "mc_idle_right";

	// Public fields
	public Item CurrentItem { get; set; } = null;

	// Private fields
	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		PlayIdleAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovementInput();
		HandleInteractionInput();
	}


	// === Movement ===
	private void HandleMovementInput()
	{
		Vector2 inputDirection = Input.GetVector(
			MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction
		);

		if (inputDirection != Vector2.Zero)
		{
			CurrentFacing = FacingDirectionFromVector(inputDirection);

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
		string animName = CurrentFacing switch
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
		string animName = CurrentFacing switch
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
		string animName = CurrentFacing switch
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

		switch (CurrentFacing)
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

		if (CurrentItem != null)
		{
			UseItemOn(CurrentItem, target);
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
}