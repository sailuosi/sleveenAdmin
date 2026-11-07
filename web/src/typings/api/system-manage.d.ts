declare namespace Api {
  namespace SystemManage {
    type GuidRecord = {
      id: string;
    };

    type User = GuidRecord & {
      account: string;
      realName: string | null;
      status: number;
      orgId: string | null;
      createTime: string;
      updateTime: string | null;
    };

    type UserInput = {
      id?: string | null;
      account: string;
      password?: string | null;
      realName?: string | null;
      status: number;
      orgId?: string | null;
    };

    type Menu = GuidRecord & {
      pid: string | null;
      name: string;
      title: string;
      path: string | null;
      component: string | null;
      icon: string | null;
      type: number;
      orderNo: number;
      permission: string | null;
      isHide: boolean;
      createTime: string;
      menuCode?: string | null;
      menuType?: string | null;
      redirect?: string | null;
      outLink?: string | null;
      isIframe?: boolean;
      isKeepAlive?: boolean;
      isAffix?: boolean;
      status?: number;
      remark?: string | null;
      extPara?: string | null;
      menuTheme?: string | null;
      isApp?: boolean;
      children?: Menu[];
    };

    type MenuInput = {
      id?: string | null;
      pid?: string | null;
      name: string;
      title: string;
      path?: string | null;
      component?: string | null;
      icon?: string | null;
      type: number;
      orderNo: number;
      permission?: string | null;
      isHide: boolean;
      menuCode?: string | null;
      menuType?: string | null;
      redirect?: string | null;
      outLink?: string | null;
      isIframe?: boolean;
      isKeepAlive?: boolean;
      isAffix?: boolean;
      status?: number;
      remark?: string | null;
      extPara?: string | null;
      menuTheme?: string | null;
      isApp?: boolean;
    };

    type Org = GuidRecord & {
      pid: string | null;
      name: string;
      code: string | null;
      classCode?: string | null;
      orderNo: number;
      createTime: string;
      children?: Org[];
    };

    type OrgInput = {
      id?: string | null;
      pid?: string | null;
      name: string;
      code?: string | null;
      orderNo: number;
    };

    type DictType = GuidRecord & {
      code: string;
      name: string;
      remark: string | null;
    };

    type DictTypeInput = {
      id?: string | null;
      code: string;
      name: string;
      remark?: string | null;
    };

    type DictData = GuidRecord & {
      typeId: string;
      label: string;
      value: string;
      orderNo: number;
    };

    type DictDataInput = {
      id?: string | null;
      typeId: string;
      label: string;
      value: string;
      orderNo: number;
    };

    type Config = GuidRecord & {
      code: string;
      value: string | null;
      remark: string | null;
    };

    type ConfigInput = {
      id?: string | null;
      code: string;
      value?: string | null;
      remark?: string | null;
    };
  }
}
