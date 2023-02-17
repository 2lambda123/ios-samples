// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;
using AVFoundation;

namespace ScanningAndDetecting3DObjects {
	internal partial class FlashlightButton : UIButton {
		internal FlashlightButton (IntPtr handle) : base (handle)
		{
		}

		private bool toggledOn;

		internal bool ToggledOn {
			get => toggledOn;
			set {
				toggledOn = value;
				// Update UI
				if (toggledOn) {
					SetTitle ("Light Off", UIControlState.Normal);
					BackgroundColor = Utilities.AppLightBlue;
				} else {
					SetTitle ("Light On", UIControlState.Normal);
					BackgroundColor = Utilities.AppLightBlue;
				}

				// Toggle flashlight
				var captureDevice = AVCaptureDevice.GetDefaultDevice (AVMediaTypes.Video);
				if (captureDevice == null) {
					return;
				}
				if (!captureDevice.HasTorch) {
					if (toggledOn) {
						toggledOn = false;
					}
					return;
				}

				try {
					NSError err = null;
					var locked = captureDevice.LockForConfiguration (out err);
					if (!locked || err != null) {
						Console.WriteLine ("Error while attempting to access flashlight.");
						return;
					}
					var mode = toggledOn ? AVCaptureTorchMode.On : AVCaptureTorchMode.Off;
					if (captureDevice.IsTorchModeSupported (mode)) {
						captureDevice.TorchMode = mode;
					}
					captureDevice.UnlockForConfiguration ();
				} catch (Exception x) {
					Console.WriteLine ("Error while attempting to access flashlight.");
				}
			}
		}
	}
}
