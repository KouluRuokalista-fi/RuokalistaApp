using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuokalistaApp.Platforms.Android
{
	internal class RequestNotificationPerms
	{

		public static async Task<PermissionStatus> RequestNotificationPermission()
		{
			PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

			if (status == PermissionStatus.Granted)
				return status;

			if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
			{
				// Prompt the user to turn on in settings
				// On iOS once a permission has been denied it may not be requested again from the application
				return status;
			}

			if (Permissions.ShouldShowRationale<Permissions.PostNotifications>())
			{
				// Prompt the user with additional information as to why the permission is needed
			}

			status = await Permissions.RequestAsync<Permissions.PostNotifications>();

			return status;
		}
	}
}
