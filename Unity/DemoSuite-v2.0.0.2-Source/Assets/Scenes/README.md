# Scene Loading Info

* Load the PersistentScene First. This contains basic Environment, User, and Camera Components.
  * NeurableUser establishes a connection to the Neurable Headset. We suggest keeping this Component Persistent between scenes as User calibration and connection will be lost upon destruction.
  * The Scene I/O Controller Object in PersistentScene choreographs most Scene Management and global event triggers.
* Drag desired Scene into the Hierarchy, below the PersistentScene.
* The Trainer is built to be the First Run Scene. Most content is gated by whether or not the User has a valid Neurable Model.
  * If you are debugging a scene other than the Trainer, Go to the Scene I/O Controller object in PersistentScene, find the Watch User Component, and enable DEBUG_USER. This will activate objects as if the User is ready.
