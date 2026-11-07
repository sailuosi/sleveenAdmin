declare namespace Api {
  /**
   * namespace Auth
   *
   * backend api module: "auth"
   */
  namespace Auth {
    interface CaptchaInfo {
      captchaId: string;
      captchaImage: string;
    }

    interface LoginToken {
      token: string;
      refreshToken: string;
    }

    interface UserInfo {
      userId: string;
      account: string;
      userName: string;
      roles: string[];
      buttons: string[];
      appVersion?: string;
      updateNotes?: string;
      updateTip?: string;
      AppVersion?: string;
      UpdateNotes?: string;
      UpdateTip?: string;
    }

    /** Anonymous /api/auth/appRelease */
    interface AppRelease {
      version?: string;
      updateNotes?: string;
      updateTip?: string;
      publishTime?: string;
      Version?: string;
      AppVersion?: string;
      UpdateNotes?: string;
      UpdateTip?: string;
      PublishTime?: string;
    }
  }
}
