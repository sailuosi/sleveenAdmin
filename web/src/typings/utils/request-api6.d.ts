declare module '@/utils/requestAPI6.js' {
  import type { AxiosRequestConfig } from 'axios';

  function request<T = any>(config: AxiosRequestConfig): Promise<T>;
  export default request;
}
