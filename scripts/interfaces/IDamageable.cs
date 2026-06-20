using Godot;

public interface IDamageable
{
	void TakeDamage(int damage, Node2D source);
	bool IsDead { get; }
	void Die(Node2D source);
}
