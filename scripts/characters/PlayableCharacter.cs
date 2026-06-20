using Godot;

public partial class PlayableCharacter : CharacterBody2D, IMoveable
{
	// Action constants
	private const string MoveUpAction = "move_up";
    private const string MoveDownAction = "move_down";
    private const string MoveLeftAction = "move_left";
    private const string MoveRightAction = "move_right";
	private const string RunAction = "run";


	// Movement properties
	[Export] public float Speed = 150.0f;
	[Export] public float RunSpeed = 250.0f;
	[Export] public FacingDirection CurrentFacing = FacingDirection.DOWN;

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


	// Private fields
	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		PlayIdleAnimation();
	}

    public override void _PhysicsProcess(double delta)
	{
		HandleMovementInput();
	}

	private void HandleMovementInput()
	{
		Vector2 inputDirection = Input.GetVector(
			MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction
		);

		if (inputDirection != Vector2.Zero)
		{
			CurrentFacing = FacingDirectionFromVector(inputDirection);

			bool isRunning = Input.IsActionPressed(RunAction);
			if(isRunning)
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


    private void Examine(IInteractable target)
	{
		target.Interact();
	}              
    
	private void UseItemOn(Item item, IInteractable target)
	{
		target.InteractWithItem(item);
	}
}
