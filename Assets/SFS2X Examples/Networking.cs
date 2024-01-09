using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Logging;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;

public class Networking : MonoBehaviour {

	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Tooltip("IP address or domain name of the SmartFoxServer 2X instance")]
	public string Host = "127.0.0.1";

	[Tooltip("TCP port listened by the SmartFoxServer 2X instance; used for regular socket connection in all builds except WebGL")]
	public int TcpPort = 9933;

	[Tooltip("WebSocket port listened by the SmartFoxServer 2X instance; used for in WebGL build only")]
	public int WSPort = 8080;

	[Tooltip("Name of the SmartFoxServer 2X Zone to join")]
	public string Zone = "MultiverseHubEU";

	private SmartFox sfs;
	private bool firstJoin = true;

	private string currentState = "Available";

	private static string BUDDYVAR_AGE = SFSBuddyVariable.OFFLINE_PREFIX + "age";
	private static string BUDDYVAR_MOOD = "mood";

	public static Networking instance { get; protected set; }

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Start() {

		errorText.text = "";
		nameInput.text = "";

		if (instance == null)
			instance = this;
	}

	void Update() {
		// As Unity is not thread safe, we process the queued up callbacks on every frame
		if (sfs != null)
			sfs.ProcessEvents();
	}

	void OnApplicationQuit() {
		// Always disconnect before quitting
		if (sfs != null && sfs.IsConnected)
			sfs.Disconnect ();
	}




	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	// Login panel components
	public GameObject loginPanel;
	public InputField nameInput;
	public Button loginButton;
	public Text errorText;

	//Chat



	// User details panel components
	public GameObject GameUIContainer;
	public GameObject userPanel;
	public Text loggedInText;
	public Toggle onlineToggle;
	public InputField nickInput;
	public InputField ageInput;
	public InputField moodInput;
	public Text stateButtonLabel;
	public RectTransform stateDropDown;
	public GameObject stateItemPrefab;


	// Bubby list panel components
	public GameObject buddiesPanel;
	public InputField buddyInput;
	public RectTransform buddyListContent;
	public GameObject buddyListItemPrefab;
	public Sprite IconAvailable;
	public Sprite IconAway;
	public Sprite IconOccupied;
	public Sprite IconOffline;
	public Sprite IconBlocked;
	public Sprite IconBlock;
	public Sprite IconUnblock;

	// Chat panel components
	//Buddy
	public RectTransform chatPanelsContainer;
	public GameObject chatPanelPrefab;

	//Room
	//public ScrollRect chatScrollView;
	////public Text chatText;
	//public CanvasGroup chatControls;

	//public Text userListText;
	public Transform roomListContent;
	public GameObject roomListItem;
	public GameObject roomsPanel;


	private Dictionary<SFSUser, GameObject> remotePlayers = new Dictionary<SFSUser, GameObject>();
	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------

	public void OnLoginButtonClick() {
		//enableLoginUI(false);

		// Set connection parameters
		ConfigData cfg = new ConfigData();
		cfg.Host = Host;
		#if !UNITY_WEBGL
		cfg.Port = TcpPort;
		#else
		cfg.Port = WSPort;
		#endif
		cfg.Zone = Zone;

		// Initialize SFS2X client and add listeners
		#if !UNITY_WEBGL
		sfs = new SmartFox();
		#else
		sfs = new SmartFox(UseWebSocket.WS_BIN);
	#endif

		Debug.Log("SFS2X C# API v" + sfs.Version);

		// Add SFS2X event listeners
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
		sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdd);
		sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);

		// Connect to SFS2X
		sfs.Connect(cfg);

		

	}

	public void OnRoomItemClick(int roomId)
	{
		sfs.Send(new Sfs2X.Requests.JoinRoomRequest(roomId));
	}

	/**
	 * Makes user panel slide in/out.
	 */
	public void OnUserTabClick()
	{
		if(userPanel.activeSelf)
			userPanel.SetActive(false);
		else
			userPanel.SetActive(true);
	}

	/**
	 * Makes buddies panel slide in/out.
	 */
	public void OnBuddiesTabClick()
	{

		if (buddiesPanel.activeSelf)
			buddiesPanel.SetActive(false);
		else
			buddiesPanel.SetActive(true);


	}

	/**
	 * Changes the currently selected state in user panel.
	 */
	public void OnStateItemClick(string stateValue)
	{
		currentState = stateValue;
		stateButtonLabel.text = currentState;
		stateDropDown.gameObject.SetActive(false);
	}

	/**
	 * Makes current user go online/offline in the buddy list system.
	 */
	public void OnOnlineToggleChange(bool isChecked)
	{
		sfs.Send(new Sfs2X.Requests.Buddylist.GoOnlineRequest(isChecked));
	}

	/**
	 * Sets the current user details in the buddy system.
	 * This can be done if the current user is online in the buddy system only.
	 */
	public void OnSetDetailsButtonClick()
	{
		List<BuddyVariable> buddyVars = new List<BuddyVariable>();
		buddyVars.Add(new SFSBuddyVariable(ReservedBuddyVariables.BV_NICKNAME, nickInput.text));
		buddyVars.Add(new SFSBuddyVariable(BUDDYVAR_AGE, Convert.ToInt32(ageInput.text)));
		buddyVars.Add(new SFSBuddyVariable(BUDDYVAR_MOOD, moodInput.text));
		buddyVars.Add(new SFSBuddyVariable(ReservedBuddyVariables.BV_STATE, currentState));
		
		sfs.Send(new Sfs2X.Requests.Buddylist.SetBuddyVariablesRequest(buddyVars));
	}

	/**
	* Adds a buddy to the current user's buddy list.
	*/
	public void OnAddBuddyButtonClick()
	{
		if (buddyInput.text != "")
		{
			sfs.Send(new Sfs2X.Requests.Buddylist.AddBuddyRequest(buddyInput.text));
			buddyInput.text = "";
		}
	}

	/**
	 * Start a chat with a buddy.
	 */
	public void OnChatBuddyButtonClick(string buddyName)
	{
		// Check if panel is already open; if yes bring it to front
		Transform panel = chatPanelsContainer.Find(buddyName);

		if (panel == null)
		{
			GameObject newChatPanel = Instantiate(chatPanelPrefab) as GameObject;
			ChatPanel chatPanel = newChatPanel.GetComponent<ChatPanel>();

			chatPanel.buddy = sfs.BuddyManager.GetBuddyByName(buddyName);
			chatPanel.closeButton.onClick.AddListener(() => OnChatCloseButtonClick(buddyName));
			chatPanel.sendButton.onClick.AddListener(() => OnSendMessageButtonClick(buddyName));
			chatPanel.messageInput.onEndEdit.AddListener(val => OnSendMessageKeyPress(buddyName));

			/*
			chatPanel.messageInput.onEndEdit.AddListener(val =>
				{
					if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
						Debug.Log("End edit on enter");
				});
	*/
			newChatPanel.transform.SetParent(chatPanelsContainer, false);

		}
		else
		{
			panel.SetAsLastSibling();
		}
	}

	/**
	 * Sends a chat message to a buddy when SEND button is pressed.
	 */
	public void OnSendMessageButtonClick(string buddyName)
	{
		// Get panel
		Transform panel = chatPanelsContainer.Find(buddyName);

		if (panel != null)
		{
			ChatPanel chatPanel = panel.GetComponent<ChatPanel>();

			string message = chatPanel.messageInput.text;

			// Add a custom parameter containing the recipient name,
			// so that we are able to write messages in the proper chat tab
			ISFSObject _params = new SFSObject();
			_params.PutUtfString("recipient", buddyName);

			Buddy buddy = sfs.BuddyManager.GetBuddyByName(buddyName);

			sfs.Send(new Sfs2X.Requests.Buddylist.BuddyMessageRequest(message, buddy, _params));

			chatPanel.messageInput.text = "";
			chatPanel.messageInput.ActivateInputField();
			chatPanel.messageInput.Select();
		}
	}

	/**
	* Sends a chat message to a buddy when ENTER key is pressed.
	*/
	public void OnSendMessageKeyPress(string buddyName)
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			OnSendMessageButtonClick(buddyName);
	}

	/**
	 * Destroys a chat panel.
	 */
	public void OnChatCloseButtonClick(string panelName)
	{
		Transform panel = chatPanelsContainer.Find(panelName);
		if (panel != null)
			UnityEngine.Object.Destroy(panel.gameObject);
	}

	/**
	 * Blocks/unblocks a buddy.
	 */
	public void OnBlockBuddyButtonClick(string buddyName)
	{
		bool isBlocked = sfs.BuddyManager.GetBuddyByName(buddyName).IsBlocked;

		sfs.Send(new Sfs2X.Requests.Buddylist.BlockBuddyRequest(buddyName, !isBlocked));
	}

	/**
	 * Removes a user from the buddy list.
	 */
	public void OnRemoveBuddyButtonClick(string buddyName)
	{
		sfs.Send(new Sfs2X.Requests.Buddylist.RemoveBuddyRequest(buddyName));
	}



	//----------------------------------------------------------
	// Private helper methods
	//----------------------------------------------------------


	private void reset() {
		// Remove SFS2X listeners
		sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
		sfs.RemoveEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
		sfs.RemoveEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.RemoveEventListener(SFSEvent.ROOM_ADD, OnRoomAdd);
		sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
	//	sfs.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);

		sfs = null;
		
		// Enable interface
		//enableInterface(true);
	}

	private void populateRoomList(List<Room> rooms)
	{
		// Clear current Room list
		clearRoomList();

		// For the roomlist we use a scrollable area containing a separate prefab button for each Room
		// Buttons are clickable to join Rooms
		foreach (Room room in rooms)
		{
			int roomId = room.Id;

			GameObject newListItem = Instantiate(roomListItem) as GameObject;
			RoomItem roomItem = newListItem.GetComponent<RoomItem>();
			roomItem.nameLabel.text = room.Name;
			roomItem.maxUsersLabel.text = "[max " + room.MaxUsers + " users]";
			roomItem.roomId = roomId;

			roomItem.button.onClick.AddListener(() => OnRoomItemClick(roomId));

			newListItem.transform.SetParent(roomListContent, false);
		}
	}

	private void clearRoomList()
	{
		foreach (Transform child in roomListContent.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}
	private void populateUserList(List<User> users)
	{
		// For the userlist we use a simple text area, with a user name in each row
		// No interaction is possible in this example

		// Get user names
		List<string> userNames = new List<string>();

		foreach (User user in users)
		{

			string name = user.Name;

			if (user == sfs.MySelf)
				name += " <color=#808080ff>(you)</color>";

			userNames.Add(name);
		}

		// Sort list
		userNames.Sort();

		// Display list
		//userListText.text = "";
		//userListText.text = String.Join("\n", userNames.ToArray());
	}


	public void setAvatarPositionVariables(int x, int y, int dir)
	{

		List<UserVariable> userVars = new List<UserVariable>();

		userVars.Add(new SFSUserVariable("USERVAR_X", x));
		userVars.Add(new SFSUserVariable("USERVAR_Y", y));
		userVars.Add(new SFSUserVariable("USERVAR_DIR", dir));

		sfs.Send(new SetUserVariablesRequest(userVars));
			
	}


	private void OnUserVariableUpdate(BaseEvent evt)
	{
		List<String> changedVars = (List<String>)evt.Params["changedVars"];
		SFSUser user = (SFSUser)evt.Params["user"];
		
		// Check if the user changed his x and y User Variables
		if (changedVars.Contains("USERVAR_X") || changedVars.Contains("USERVAR_Y"))
		{

			// Check if avatar exists
			if (!remotePlayers.ContainsKey(user))
			//{
				// Move the user avatar
			//	moveAvatar(user);
	
			//}
			//else
			//{
				// Create the user avatar
			//	createAvatar(user, true);
	
			//}

			// Move the character to a new position...


			Debug.Log("OnUserVarsUpdate");
		}
	}



	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------

	private void OnLogin(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];

		// Hide login panel
		loginPanel.SetActive(false);

		// Show user and buddies panel tabs
		GameUIContainer.SetActive(true);
		//userPanel.SetBool("loggedIn", true);
	//	buddiesPanel.SetBool("loggedIn", true);

		// Set "Logged in as" text
		loggedInText.text = "Logged in as " + user.Name;

		// Show system message
		string msg = "Connection established successfully\n";
		msg += "SFS2X API version: " + sfs.Version + "\n";
		msg += "Connection mode is: " + sfs.ConnectionMode + "\n";
		msg += "Logged in as " + user.Name;
		printSystemMessage(msg);

		// Populate Room list
		populateRoomList(sfs.RoomList);

		// Initialize buddy list system
		sfs.Send(new Sfs2X.Requests.Buddylist.InitBuddyListRequest());

		if (sfs.RoomList.Count > 0)
		{
			//Join Lobby Room
			sfs.Send(new Sfs2X.Requests.JoinRoomRequest(sfs.RoomList[0].Name));
		}
	}

	private void OnLoginError(BaseEvent evt)
	{
		// Disconnect
		sfs.Disconnect();

		// Remove SFS2X listeners and re-enable interface
		reset();

		// Show error message
		errorText.text = "Login failed: " + (string)evt.Params["errorMessage"];
	}


	private void OnConnection(BaseEvent evt)
	{
		if ((bool)evt.Params["success"])
		{
			Debug.Log("SFS2X API version: " + sfs.Version);
			Debug.Log("Connection mode is: " + sfs.ConnectionMode);

			// Login
			sfs.Send(new Sfs2X.Requests.LoginRequest(nameInput.text));
		}
		else
		{
			// Remove SFS2X listeners and re-enable interface
			reset();

			// Show error message
			errorText.text = "Connection failed; is the server running at all?";
		}
	}

	private void OnConnectionLost(BaseEvent evt)
	{
		// Show login panel
		loginPanel.SetActive(true);

		// Hide user and buddies panels
		GameUIContainer.SetActive(false);
	

		// Remove SFS2X listeners and re-enable interface
		reset();

		string reason = (string)evt.Params["reason"];

		if (reason != ClientDisconnectionReason.MANUAL)
		{
			// Show error message
			errorText.text = "Connection was lost; reason is: " + reason;
		}
	}



	//----------------------------------------------------------
	// SmartFoxServer Buddy event listeners
	//----------------------------------------------------------

	private void OnBuddyError(BaseEvent evt)
	{
		Debug.LogError("The following error occurred in the buddy list system: " + (string)evt.Params["errorMessage"]);
	}

	private void OnBuddyListInit(BaseEvent evt)
	{
		// Populate list of buddies
		OnBuddyListUpdate(evt);

		// Set current user details as buddy

		// Nick
		nickInput.text = (sfs.BuddyManager.MyNickName != null ? sfs.BuddyManager.MyNickName : "");

		// States
		foreach (string state in sfs.BuddyManager.BuddyStates)
		{
			string stateValue = state;
			GameObject newDropDownItem = Instantiate(stateItemPrefab) as GameObject;
			BuddyStateItemButton stateItem = newDropDownItem.GetComponent<BuddyStateItemButton>();
			stateItem.stateValue = stateValue;
			stateItem.label.text = stateValue;

			stateItem.button.onClick.AddListener(() => OnStateItemClick(stateValue));

			newDropDownItem.transform.SetParent(stateDropDown, false);

			// Set current state
			if (sfs.BuddyManager.MyState == state)
			{
				OnStateItemClick(state);
			}
		}

		// Online
		onlineToggle.isOn = sfs.BuddyManager.MyOnlineState;

		// Buddy variables
		BuddyVariable age = sfs.BuddyManager.GetMyVariable(BUDDYVAR_AGE);
		ageInput.text = ((age != null && !age.IsNull()) ? Convert.ToString(age.GetIntValue()) : "");

		BuddyVariable mood = sfs.BuddyManager.GetMyVariable(BUDDYVAR_MOOD);
		moodInput.text = ((mood != null && !mood.IsNull()) ? mood.GetStringValue() : "");
	}

	/**
	 * Populates the buddy list.
	 */
	private void OnBuddyListUpdate(BaseEvent evt)
	{

		// Remove current list content
		for (int i = buddyListContent.childCount - 1; i >= 0; --i)
		{
			GameObject.Destroy(buddyListContent.GetChild(i).gameObject);
		}
		buddyListContent.DetachChildren();

		// Recreate list content
		foreach (Buddy buddy in sfs.BuddyManager.BuddyList)
		{
			GameObject newListItem = Instantiate(buddyListItemPrefab) as GameObject;

			BuddyListItem buddylistItem = newListItem.GetComponent<BuddyListItem>();

			// Nickname
			buddylistItem.mainLabel.text = (buddy.NickName != null && buddy.NickName != "") ? buddy.NickName : buddy.Name;

			// Age
			BuddyVariable age = buddy.GetVariable(BUDDYVAR_AGE);
			buddylistItem.mainLabel.text += (age != null && !age.IsNull()) ? " (" + age.GetIntValue() + " yo)" : "";

			// Mood
			BuddyVariable mood = buddy.GetVariable(BUDDYVAR_MOOD);
			buddylistItem.moodLabel.text = (mood != null && !mood.IsNull()) ? mood.GetStringValue() : "";

			// Icon
			if (buddy.IsBlocked)
			{
				buddylistItem.stateIcon.sprite = IconBlocked;
				buddylistItem.chatButton.interactable = false;
				buddylistItem.blockButton.transform.GetChild(0).GetComponentInChildren<Image>().sprite = IconUnblock;
			}
			else
			{
				buddylistItem.blockButton.transform.GetChild(0).GetComponentInChildren<Image>().sprite = IconBlock;

				if (!buddy.IsOnline)
				{
					buddylistItem.stateIcon.sprite = IconOffline;
					buddylistItem.chatButton.interactable = false;
				}
				else
				{
					string state = buddy.State;

					if (state == "Available")
						buddylistItem.stateIcon.sprite = IconAvailable;
					else if (state == "Away")
						buddylistItem.stateIcon.sprite = IconAway;
					else if (state == "Occupied")
						buddylistItem.stateIcon.sprite = IconOccupied;
				}
			}

			// Buttons
			string buddyName = buddy.Name; // Required or the listeners will always receive the last buddy name
			buddylistItem.removeButton.onClick.AddListener(() => OnRemoveBuddyButtonClick(buddyName));
			buddylistItem.blockButton.onClick.AddListener(() => OnBlockBuddyButtonClick(buddyName));
			buddylistItem.chatButton.onClick.AddListener(() => OnChatBuddyButtonClick(buddyName));

			buddylistItem.buddyName = buddyName;

			// Add item to list
			newListItem.transform.SetParent(buddyListContent, false);

			// Also update chat panel if open
			Transform panel = chatPanelsContainer.Find(buddyName);

			if (panel != null)
			{
				ChatPanel chatPanel = panel.GetComponent<ChatPanel>();
				chatPanel.buddy = buddy;
			}
		}
	}

	/**
 * Handles messages receive from buddies.
 */
	private void OnBuddyMessage(BaseEvent evt)
	{
		bool isItMe = (bool)evt.Params["isItMe"];
		Buddy sender = (Buddy)evt.Params["buddy"];
		string message = (string)evt.Params["message"];

		Buddy buddy;
		if (isItMe)
		{
			string buddyName = (evt.Params["data"] as ISFSObject).GetUtfString("recipient");
			buddy = sfs.BuddyManager.GetBuddyByName(buddyName);
		}
		else
			buddy = sender;

		if (buddy != null)
		{
			// Open panel if needed
			OnChatBuddyButtonClick(buddy.Name);

			// Print message
			Transform panel = chatPanelsContainer.Find(buddy.Name);
			ChatPanel chatPanel = panel.GetComponent<ChatPanel>();
			chatPanel.addMessage("<b>" + (isItMe ? "You" : buddy.Name) + ":</b> " + message);
		}
	}

	//----------------------------------------------------------
	// SmartFoxServer room event listeners
	//----------------------------------------------------------
	private void OnRoomJoin(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];

		// Clear chat (uless this is the first time a Room is joined - or the initial system message would be deleted)
		if (!firstJoin)
			//chatText.text = "";

		firstJoin = false;

		int x = 0;

		int y = 0;

		int dir = 0;


		gameManager.instance.LoadPlayerRoom();

		//load room scene first
		


		// Show system message
		printSystemMessage("\nYou joined room '" + room.Name + "'\n");

		// Enable chat controls
		//chatControls.interactable = true;

		// Populate users list
		populateUserList(room.UserList);
	}

	private void OnRoomJoinError(BaseEvent evt)
	{
		// Show error message
		printSystemMessage("Room join failed: " + (string)evt.Params["errorMessage"]);
	}

	private void OnPublicMessage(BaseEvent evt)
	{
		User sender = (User)evt.Params["sender"];
		string message = (string)evt.Params["message"];

		printUserMessage(sender, message);
	}



	private void OnUserEnterRoom(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];
		Room room = (Room)evt.Params["room"];

		// Show system message
		printSystemMessage("User " + user.Name + " entered the room");

		// Populate users list
		populateUserList(room.UserList);
	}

	private void OnUserExitRoom(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];

		if (user != sfs.MySelf)
		{
			Room room = (Room)evt.Params["room"];

			// Show system message
			printSystemMessage("User " + user.Name + " left the room");

			// Populate users list
			populateUserList(room.UserList);
		}
	}

	private void OnRoomAdd(BaseEvent evt)
	{
		// Re-populate Room list
		populateRoomList(sfs.RoomList);
	}


	//----------------------------------------------------------
	// SmartFoxServer chat 
	//----------------------------------------------------------



	// In player room only.
	private void printUserMessage(User user, string message)
	{
		//chatText.text += "<b>" + (user == sfs.MySelf ? "You" : user.Name) + ":</b> " + message + "\n";

		//Canvas.ForceUpdateCanvases();

		// Scroll view to bottom
		//chatScrollView.verticalNormalizedPosition = 0;
	}

	private void printSystemMessage(string message)
	{
		//chatText.text += "<color=#808080ff>" + message + "</color>\n";
		//Canvas.ForceUpdateCanvases();

		// Scroll view to bottom
		//chatScrollView.verticalNormalizedPosition = 0;
	}

}
