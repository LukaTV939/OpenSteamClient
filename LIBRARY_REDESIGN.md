# Library redesign
The current looks bad, is not very modular, would work terribly with plugins, doesn't show a lot of important info, is not well thought out, and hence the redesign.

- Would initially start out with the fullscreen view, and then once a game is clicked show the infopanel. 
- [ ] Full screen "browse" view (BrowseView)
	- [ ] List of icon, game name (SimpleBrowse)
		- [ ] Name color based on install, playing and cloud sync state
		- [ ] Columns for install size, playtime, etc, configurable by the user
	- [ ] Grid view with portraits (PortraitGridBrowse)
- [ ] Panel for viewing game information and launching (InfoPanel)
	- [ ] Current game patch (PatchInfolet)
	- [ ] ProtonDB info (ProtonDBInfolet)
	- [ ] Friends who play (FriendsInfolet)
	- [ ] Total and last two weeks playtime (PlaytimeInfolet)
	- [ ] Size, split by DLC, Mods, etc. (InstallInfolet)
	- [ ] Installed mods (ModsInfolet)