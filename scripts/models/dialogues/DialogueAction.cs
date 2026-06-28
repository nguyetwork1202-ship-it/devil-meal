public enum DialogueAction
{
	NONE,              // ActionTarget = null, use to navigate to other options
	END_DIALOGUE,      // ActionTarget = null, end current dialogue
	START_DIALOGUE,    // ActionTarget = dialogue id
	GIVE_ITEM,         // ActionTarget = item id
	START_CUTSCENE     // ActionTarget = cutscene id
}
